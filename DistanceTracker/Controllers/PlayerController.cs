using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class PlayerController : Controller
	{
		public PlayerController(LeaderboardEntryDAL leDAL, LeaderboardEntryHistoryDAL lehDAL, PlayerDAL playerDAL, LeaderboardDAL lDAL, SteamDAL steamDAL)
		{
			EntryDAL = leDAL;
			HistoryDAL = lehDAL;
			PlayerDAL = playerDAL;
			LeaderDAL = lDAL;
			SteamDAL = steamDAL;
		}

		public LeaderboardEntryDAL EntryDAL { get; }
		public LeaderboardEntryHistoryDAL HistoryDAL { get; }
		public PlayerDAL PlayerDAL { get; }
		public LeaderboardDAL LeaderDAL { get; }
		public SteamDAL SteamDAL { get; }

		public async Task<IActionResult> Index(ulong steamID)
		{
			var player = await PlayerDAL.GetPlayer(steamID);

			return View(player);
		}

		public async Task<IActionResult> GetGlobalStats(ulong steamID)
		{
			var officialLeaderboards = await LeaderDAL.GetLeaderboards(true);
			var officalLeaderboardIDs = officialLeaderboards.Select(x => x.ID).ToList();

			var globalRanking = await EntryDAL.GetGlobalRankingForPlayer(steamID);

			var lastWeeksImprovements = await HistoryDAL.GetPastWeeksImprovements(steamID, officalLeaderboardIDs);
			var pointsImprovement = NoodlePointsUtil.CalculateImprovement(lastWeeksImprovements);
			var oldGlobalRank = await EntryDAL.GetGlobalRankingForPoints((int)globalRanking.NoodlePoints - pointsImprovement);
			var funStats = await PlayerDAL.GetFunStats(steamID);

			var stats = new PlayerGlobalStats
			{
				GlobalLeaderboardEntry = globalRanking,
				LastWeeksRankImprovement = oldGlobalRank.Rank - globalRanking.Rank,
				LastWeeksPointsImprovement = pointsImprovement,
				LastWeeksRatingImprovement = (double)pointsImprovement / (NoodlePointsUtil.MAX_POINTS_PER_MAP * NoodlePointsUtil.NUM_OFFICIAL_SPRINTS) * 100,
				FunStats = funStats,
			};

			return new JsonResult(stats);
		}

		public async Task<IActionResult> GetLeaderboardRankings(ulong steamID)
		{
			var rankedLeaderboardEntries = await EntryDAL.GetRankedLeaderboardEntriesForPlayer(steamID);
			rankedLeaderboardEntries = rankedLeaderboardEntries
				.OrderBy(x => x.Rank)
				.ToList();

			return new JsonResult(rankedLeaderboardEntries);
		}

		public async Task<IActionResult> GetRecentActivity(ulong steamID)
		{
			var recentFirstSightings = await EntryDAL.GetRecentFirstSightings(numRows: 40, steamID: steamID);
			var recentImprovements = await HistoryDAL.GetRecentImprovements(numRows: 40, steamID: steamID);

			var recentActivity = new List<Activity>();
			recentFirstSightings.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.FirstSeenTimeUTC,
				Sighting = x,
			}));
			recentImprovements.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.UpdatedTimeUTC,
				Improvement = x,
			}));

			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).Take(30).ToList();
			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).Take(40).ToList();

			return new JsonResult(recentActivity);
		}

		public async Task<IActionResult> Compare(string steamIDs)
		{
			if (string.IsNullOrWhiteSpace(steamIDs))
			{
				return View(null);
			}

			var leaderboards = await LeaderDAL.GetAllLeaderboards();

			// Make sure the list of steam IDs is unique
			var steamIDList = steamIDs.Split(",").Select(x => ulong.Parse(x)).ToList();
			steamIDList = steamIDList.GroupBy(x => x).Select(x => x.First()).ToList();
			var entriesList = new Dictionary<ulong, List<RankedLeaderboardEntry>>();
			var players = new List<Player>();

			foreach (var steamID in steamIDList)
			{
				entriesList.Add(steamID, await EntryDAL.GetRankedLeaderboardEntriesForPlayer(steamID));
				players.Add(await PlayerDAL.GetPlayer(steamID));
			}

			var viewModel = new PlayerComparisonViewModel()
			{
				Comparisons = new List<PlayerComparisonEntry>(),
				Players = players,
				SteamIDs = steamIDs,
			};

			foreach (var leaderboard in leaderboards.OrderBy(x => x.ID))
			{
				var comparison = new PlayerComparisonEntry();
				comparison.Leaderboard = leaderboard;
				comparison.RankedEntries = new Dictionary<ulong, RankedLeaderboardEntry>();
				foreach (var player in players)
				{
					var entry = entriesList[player.SteamID].FirstOrDefault(x => x.Leaderboard.ID == leaderboard.ID);
					comparison.RankedEntries.Add(player.SteamID, entry);
				}
				viewModel.Comparisons.Add(comparison);
			}

			return View(viewModel);
		}

		public async Task<IActionResult> GetHistogramData(ulong steamID)
		{
			var percentileRanks = await EntryDAL.GetPercentileRanks(steamID);
			var groupedHistogramDataPoints = await EntryDAL.GetHistogramDataPoints();

			var histograms = new List<HistogramViewModel>();
			foreach (var percentileRank in percentileRanks)
			{
				// Create the basic histogram info
				var histogram = new HistogramViewModel()
				{
					Percentile = percentileRank.Percentile,
					Milliseconds = percentileRank.Milliseconds,
					BucketKeys = new List<ulong>(),
					BucketCounts = new List<ulong>(),
					Leaderboard = new Leaderboard()
					{
						ID = percentileRank.LeaderboardID,
						LevelName = percentileRank.LevelName,
						LeaderboardName = percentileRank.LeaderboardName,
						LevelSet = percentileRank.LevelSet,
						ImageURL = percentileRank.ImageURL,
					},
				};

				// Create the data point dictionary for the histogram
				var histogramDataPoints = groupedHistogramDataPoints
					.First(x => x.Key == percentileRank.LeaderboardID)
					.OrderBy(x => x.BucketFloor)
					.ToList();
				foreach (var dataPoint in histogramDataPoints)
				{
					histogram.BucketKeys.Add(dataPoint.BucketFloor);
					histogram.BucketCounts.Add(dataPoint.BucketCount);
				}

				histograms.Add(histogram);
			}

			return new JsonResult(histograms.GroupBy(x => x.Leaderboard.LevelSet).ToDictionary(x => x.Key, x => x.ToList()));
		}

		public async Task<IActionResult> RefreshSteamInfo(ulong steamID)
		{
			var players = await SteamDAL.GetPlayerSummaries(steamID);
			var player = players.FirstOrDefault();
			if (player != null)
			{
				await PlayerDAL.UpdateSteamAvatar(steamID, player.Avatar);
				await PlayerDAL.UpdateSteamName(steamID, player.PersonaName);
			}

			return RedirectToAction("Index", new { steamID = steamID });
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
