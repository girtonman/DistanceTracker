﻿using DistanceTracker.DALs;
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

		public async Task<IActionResult> OfficialGlobal()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(true);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (All Official Levels)";
			viewModel.HasTimeScores = true;
			viewModel.HasStuntScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> OfficialSprint()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(true, LevelType.Sprint);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Official Sprint Levels)";
			viewModel.HasTimeScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> OfficialChallenge()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(true, LevelType.Challenge);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Official Challenge Levels)";
			viewModel.HasTimeScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> OfficialStunt()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(true, LevelType.Stunt);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Official Stunt Levels)";
			viewModel.HasStuntScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> WorkshopGlobal()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(false);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (All Workshop Levels)";
			viewModel.HasTimeScores = true;
			viewModel.HasStuntScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> WorkshopSprint()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(false, LevelType.Sprint);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Workshop Sprint Levels)";
			viewModel.HasTimeScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> WorkshopChallenge()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(false, LevelType.Challenge);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Workshop Challenge Levels)";
			viewModel.HasTimeScores = true;
			return View("Overview", viewModel);
		}

		public async Task<IActionResult> WorkshopStunt()
		{
			var leaderboards = await LeaderDAL.GetLeaderboards(false, LevelType.Stunt);
			var leaderboardIDs = leaderboards.Select(x => x.ID).ToList();
			var viewModel = await GetOverviewForLeaderboardIDs(leaderboardIDs);
			viewModel.LeaderboardName = "Global Leaderboard (Workshop Stunt Levels)";
			viewModel.HasStuntScores = true;
			return View("Overview", viewModel);
		}

		private async Task<OverviewLeaderboardViewModel> GetOverviewForLeaderboardIDs(List<uint> leaderboardIDs)
		{
			var viewModel = new OverviewLeaderboardViewModel
			{
				LeaderboardEntries = await EntryDAL.GetGlobalLeaderboard(leaderboardIDs),
				WinnersCircle = await EntryDAL.GetGlobalWinnersCircle(leaderboardIDs),
				OptimalTotalTime = await EntryDAL.GetOptimalTotal(leaderboardIDs),
				OptimalTotalStuntScore = await EntryDAL.GetOptimalTotal(leaderboardIDs, true),
				WRLog = await HistoryDAL.GetWRLog(leaderboardIDs: leaderboardIDs),
			};

			// Add global time improvements to the entries
			var globalTimeImprovements = await HistoryDAL.GetPastWeeksImprovement(leaderboardIDs: leaderboardIDs);
			foreach (var entry in viewModel.LeaderboardEntries)
			{
				var steamID = entry.Player.SteamID;
				if (globalTimeImprovements.ContainsKey(steamID))
				{
					entry.LastWeeksTimeImprovement = globalTimeImprovements[steamID].Item1;
					entry.LastWeeksScoreImprovement = globalTimeImprovements[steamID].Item2;
				}
			}

			return viewModel;
		}

		public async Task<IActionResult> Level(uint leaderboardID)
		{
			var leaderboard = await LeaderDAL.GetLeaderboard(leaderboardID);

			var leaderboardEntries = await EntryDAL.GetRankedLeaderboardEntriesForLevel(leaderboardID, leaderboard.IsReverseRankOrder);
			var recentNewSightings = await EntryDAL.GetRecentFirstSightings(numRows: 30, leaderboardIDs: new List<uint>() { leaderboardID });
			var recentImprovements = await HistoryDAL.GetRecentImprovements(numRows: 30, leaderboardIDs: new List<uint>() { leaderboardID });

			var viewModel = new LeaderboardViewModel()
			{
				LeaderboardEntries = leaderboardEntries,
				RecentFirstSightings = recentNewSightings,
				RecentImprovements = recentImprovements,
				Leaderboard = leaderboard,
			};

			return View(viewModel);
		}

		public IActionResult Levels(string q) => View("Levels", q);

		public async Task<IActionResult> SearchLevels(string q = null)
		{
			var levels = await LeaderDAL.SearchLevels(q);
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

			var groupedLevels = levels.GroupBy(x => x.LevelSet).OrderBy(x =>
			{
				var levelSet = x.First().LevelSet;
				return levelSet != null && levelSetOrder.ContainsKey(levelSet) ? levelSetOrder[levelSet] : 99999;
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
