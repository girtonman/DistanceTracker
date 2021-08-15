﻿using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class LeaderboardController : Controller
	{
		public async Task<IActionResult> Global()
		{
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();

			var viewModel = new GlobalLeaderboardViewModel
			{
				LeaderboardEntries = await leDAL.GetGlobalLeaderboard(),
				WinnersCircle = await leDAL.GetGlobalWinnersCircle(),
				OptimalTotalTime = await leDAL.GetOptimalTotalTime(),
			};

			// Add global time improvements to the entries
			var globalTimeImprovements = await lehDAL.GetGlobalPastWeeksImprovement();
			foreach(var entry in viewModel.LeaderboardEntries)
			{
				var steamID = entry.Player.SteamID;
				if (globalTimeImprovements.ContainsKey(steamID))
				{
					entry.LastWeeksGlobalTimeImprovement = globalTimeImprovements[steamID];
				}
			}

			return View(viewModel);
		}

		public async Task<IActionResult> Level(uint leaderboardID)
		{
			var lDAL = new LeaderboardDAL();
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();
			var leaderboardEntries = await leDAL.GetRankedLeaderboardEntriesForLevel(leaderboardID);
			var recentNewSightings = await leDAL.GetRecentFirstSightings(30, null, leaderboardID);
			var recentImprovements = await lehDAL.GetRecentImprovements(30, null, leaderboardID);
			var leaderboard = await lDAL.GetLeaderboard(leaderboardID);

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
			var dal = new LeaderboardDAL();
			var levels = await dal.GetLevels();

			return new JsonResult(levels);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
