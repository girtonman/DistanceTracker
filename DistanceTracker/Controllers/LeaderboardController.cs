using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class LeaderboardController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Global()
		{
			var dal = new LeaderboardEntryDAL();
			var globalLeaderboardEntries = await dal.GetGlobalLeaderboard();

			return View(globalLeaderboardEntries);
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

		public async Task<IActionResult> Levels()
		{
			var lDAL = new LeaderboardDAL();
			var levels = await lDAL.GetLevels();

			var viewModel = new LevelsViewModel()
			{
				Levels = levels
			};

			return View(viewModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
