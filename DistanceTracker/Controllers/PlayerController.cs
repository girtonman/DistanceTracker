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
			var dal = new PlayerDAL();
			var player = await dal.GetPlayer(steamID);

			return View(player);
		}

		public async Task<IActionResult> GetGlobalStats(ulong steamID)
		{
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();

			var globalRanking = await leDAL.GetGlobalRankingForPlayer(steamID);

			var lastWeeksImprovements = await lehDAL.GetPastWeeksImprovements(steamID);
			var pointsImprovement = NoodlePointsUtil.CalculateImprovement(lastWeeksImprovements);
			var oldGlobalRank = await leDAL.GetGlobalRankingForPoints((int)globalRanking.NoodlePoints - pointsImprovement);

			var stats = new PlayerGlobalStats
			{
				GlobalLeaderboardEntry = globalRanking,
				LastWeeksRankImprovement = oldGlobalRank.Rank - globalRanking.Rank,
				LastWeeksPointsImprovement = pointsImprovement,
				LastWeeksRatingImprovement = (double)pointsImprovement / (NoodlePointsUtil.MAX_POINTS_PER_MAP * NoodlePointsUtil.NUM_OFFICIAL_SPRINTS) * 100
			};

			return new JsonResult(stats);
		}

		public async Task<IActionResult> GetLeaderboardRankings(ulong steamID)
		{
			var leDAL = new LeaderboardEntryDAL();
			var rankedLeaderboardEntries = await leDAL.GetRankedLeaderboardEntriesForPlayer(steamID);

			return new JsonResult(rankedLeaderboardEntries);
		}

		public async Task<IActionResult> GetRecentActivity(ulong steamID)
		{
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();
			var recentFirstSightings = await leDAL.GetRecentFirstSightings(10, steamID);
			var recentImprovements = await lehDAL.GetRecentImprovements(10, steamID);

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

			return new JsonResult(recentActivity);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
