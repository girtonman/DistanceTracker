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
				SteamProfilePicURL = null,
				LastWeeksPointsImprovement = pointsImprovement,
				LastWeeksRankImprovement = oldGlobalRank.Rank - globalRanking.Rank,
				LastWeeksRatingImprovement = (double) pointsImprovement / (NoodlePointsUtil.MAX_POINTS_PER_MAP * NoodlePointsUtil.NUM_OFFICIAL_SPRINTS),
				GlobalLeaderboardEntry = globalRanking,
				RecentActivity = recentActivity,
				RankedLeaderboardEntries = rankedLeaderboardEntries.OrderBy(x => x.Rank).ToList(),
			};

			return View(viewModel);
		}

		public async Task<IActionResult> Compare(ulong leftSteamID, ulong rightSteamID)
		{
			var leDAL = new LeaderboardEntryDAL();
			var lDAL = new LeaderboardDAL();
			var pDAL = new PlayerDAL();

			var leaderboards = await lDAL.GetAllLeaderboards();
			var leftEntries = await leDAL.GetRankedLeaderboardEntriesForPlayer(leftSteamID);
			var rightEntries = await leDAL.GetRankedLeaderboardEntriesForPlayer(rightSteamID);
			var leftPlayer = await pDAL.GetPlayer(leftSteamID);
			var rightPlayer = await pDAL.GetPlayer(rightSteamID);

			var viewModel = new PlayerComparisonViewModel()
			{
				LeftPlayer = leftPlayer,
				RightPlayer = rightPlayer,
				Comparisons = new List<PlayerComparisonEntry>(),
			};
			foreach(var leaderboard in leaderboards)
			{
				var leftEntry = leftEntries.FirstOrDefault(x => x.Leaderboard.ID == leaderboard.ID);
				var rightEntry = rightEntries.FirstOrDefault(x => x.Leaderboard.ID == leaderboard.ID);
				if(leftEntry != null || rightEntry != null)
				{
					viewModel.Comparisons.Add(new PlayerComparisonEntry()
					{
						Leaderboard = leaderboard,
						LeftEntry = leftEntry,
						RightEntry = rightEntry,
					});
				}
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
