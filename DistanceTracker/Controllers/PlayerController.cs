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
		public async Task<IActionResult> Index(ulong steamID)
		{
			var pDAL = new PlayerDAL();
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();

			var player = await pDAL.GetPlayer(steamID);
			var recentFirstSightings = await leDAL.GetRecentFirstSightings(10, steamID);
			var recentImprovements = await lehDAL.GetRecentImprovements(10, steamID);
			var globalRanking = await leDAL.GetGlobalRankingForPlayer(steamID);
			var rankedLeaderboardEntries = await leDAL.GetRankedLeaderboardEntriesForPlayer(steamID);

			var lastWeeksImprovements = await lehDAL.GetPastWeeksImprovements(steamID);
			var pointsImprovement = NoodlePointsUtil.CalculateImprovement(lastWeeksImprovements);
			var oldGlobalRank = await leDAL.GetGlobalRankingForPoints((int)globalRanking.NoodlePoints - pointsImprovement);

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

			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).ToList();

			var viewModel = new PlayerViewModel()
			{
				Player = player,
				LastWeeksPointsImprovement = pointsImprovement,
				LastWeeksRankImprovement = oldGlobalRank.Rank - globalRanking.Rank,
				LastWeeksRatingImprovement = (double) pointsImprovement / (NoodlePointsUtil.MAX_POINTS_PER_MAP * NoodlePointsUtil.NUM_OFFICIAL_SPRINTS) * 100,
				GlobalLeaderboardEntry = globalRanking,
				RecentActivity = recentActivity,
				RankedLeaderboardEntries = rankedLeaderboardEntries.OrderBy(x => x.Rank).ToList(),
			};

			return View(viewModel);
		}

		public async Task<IActionResult> Compare(string steamIDs)
		{
			if(string.IsNullOrWhiteSpace(steamIDs))
			{
				return View(null);
			}

			var leDAL = new LeaderboardEntryDAL();
			var lDAL = new LeaderboardDAL();
			var pDAL = new PlayerDAL();
			var leaderboards = await lDAL.GetAllLeaderboards();

			// Make sure the list of steam IDs is unique
			var steamIDList = steamIDs.Split(",").Select(x => ulong.Parse(x)).ToList();
			steamIDList = steamIDList.GroupBy(x => x).Select(x => x.First()).ToList();
			var entriesList = new Dictionary<ulong, List<RankedLeaderboardEntry>>();
			var players = new List<Player>();

			foreach (var steamID in steamIDList)
			{
				entriesList.Add(steamID, await leDAL.GetRankedLeaderboardEntriesForPlayer(steamID));
				players.Add(await pDAL.GetPlayer(steamID));
			}

			var viewModel = new PlayerComparisonViewModel()
			{
				Comparisons = new List<PlayerComparisonEntry>(),
				Players = players,
				SteamIDs = steamIDs,
			};
			foreach(var leaderboard in leaderboards.OrderBy(x => x.ID))
			{
				var comparison = new PlayerComparisonEntry();
				comparison.Leaderboard = leaderboard;
				comparison.RankedEntries = new Dictionary<ulong, RankedLeaderboardEntry>();
				foreach(var player in players)
				{
					var entry = entriesList[player.SteamID].FirstOrDefault(x => x.Leaderboard.ID == leaderboard.ID);
					comparison.RankedEntries.Add(player.SteamID, entry);
				}
				viewModel.Comparisons.Add(comparison);
			}

			return View(viewModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
