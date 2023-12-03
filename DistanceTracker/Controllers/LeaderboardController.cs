using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class LeaderboardController : Controller
	{
		public LeaderboardController(LeaderboardEntryDAL leDAL, LeaderboardEntryHistoryDAL lehDAL, LeaderboardDAL lDAL)
		{
			EntryDAL = leDAL;
			HistoryDAL = lehDAL;
			LeaderDAL = lDAL;
		}

		public LeaderboardEntryDAL EntryDAL { get; }
		public LeaderboardEntryHistoryDAL HistoryDAL { get; }
		public LeaderboardDAL LeaderDAL { get; }

		public async Task<IActionResult> Global()
		{
			var viewModel = new GlobalLeaderboardViewModel
			{
				LeaderboardEntries = await EntryDAL.GetGlobalLeaderboard(),
				WinnersCircle = await EntryDAL.GetGlobalWinnersCircle(),
				OptimalTotalTime = await EntryDAL.GetOptimalTotalTime(),
				WRLog = await HistoryDAL.GetWRLog(),
			};

			// Add global time improvements to the entries
			var globalTimeImprovements = await HistoryDAL.GetGlobalPastWeeksImprovement();
			foreach(var entry in viewModel.LeaderboardEntries)
			{
				var steamID = entry.Player.SteamID;
				if (globalTimeImprovements.ContainsKey(steamID))
				{
					entry.LastWeeksGlobalTimeImprovement = globalTimeImprovements[steamID];
				}
			}

			//var mostImprovedTime = 

			return View(viewModel);
		}

		public async Task<IActionResult> Level(uint leaderboardID)
		{
			var leaderboardEntries = await EntryDAL.GetRankedLeaderboardEntriesForLevel(leaderboardID);
			var recentNewSightings = await EntryDAL.GetRecentFirstSightings(30, null, leaderboardID);
			var recentImprovements = await HistoryDAL.GetRecentImprovements(30, null, leaderboardID);
			var leaderboard = await LeaderDAL.GetLeaderboard(leaderboardID);

			var viewModel = new LeaderboardViewModel()
			{
				LeaderboardEntries = leaderboardEntries,
				RecentFirstSightings = recentNewSightings,
				RecentImprovements = recentImprovements,
				Leaderboard = leaderboard,
			};

			return View(viewModel);
		}

		public IActionResult Levels() => View();

		public async Task<IActionResult> GetLevels()
		{
			var levels = await LeaderDAL.GetLevels();
			var levelSetOrder = new Dictionary<string, int>()
			{
				{ "Ignition", 1},
				{ "High Impact", 2},
				{ "Brute Force", 3},
				{ "Overdrive", 4},
				{ "Nightmare Fuel", 5},
				{ "Adventure", 6},
				{ "Lost to Echoes", 7},
				{ "Nexus", 8},
				{ "Legacy", 9},
			};

			var groupedLevels = levels.GroupBy(x => x.LevelSet).OrderBy(x => {
				var levelSet = x.First().LevelSet;
				return levelSetOrder.ContainsKey(levelSet) ? levelSetOrder[levelSet] : 99999;
			})
			.ThenBy(x => x.First().LevelSet);

			return new JsonResult(groupedLevels);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Oldest() => View();

		public async Task<IActionResult> GetOldestWRs()
		{
			return new JsonResult(await EntryDAL.GetOldestWRs());
		}
	}
}
