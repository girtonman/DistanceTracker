using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class HomeController : Controller
	{
		public HomeController(GeneralDAL generalDAL, LeaderboardEntryDAL leDAL, LeaderboardEntryHistoryDAL lehDAL, SteamDAL steamDAL, PlayerDAL playerDAL)
		{
			GeneralDAL = generalDAL;
			EntryDAL = leDAL;
			HistoryDAL = lehDAL;
			SteamDAL = steamDAL;
			PlayerDAL = playerDAL;
		}

		public GeneralDAL GeneralDAL { get; }
		public LeaderboardEntryDAL EntryDAL { get; }
		public LeaderboardEntryHistoryDAL HistoryDAL { get; }
		public SteamDAL SteamDAL { get; }
		public PlayerDAL PlayerDAL { get; }

		public async Task<IActionResult> IndexAsync()
		{
			var siteStats = await GeneralDAL.GetSiteStats();
			return View(siteStats);
		}

		public IActionResult GlobalActivity() => View();

		public async Task<IActionResult> GetGlobalRecentActivity()
		{
			// Get data
			var recentFirstSightings = await EntryDAL.GetRecentFirstSightings(100);
			var recentImprovements = await HistoryDAL.GetRecentImprovements(100);

			// Prepare empty view model
			var recentActivity = new List<Activity>();

			// Add data to view model
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

			// Ordering
			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).Take(100).ToList();

			// Add avatars
			foreach (var activity in recentActivity)
			{
				if (activity.Sighting != null)
				{
					await activity.Sighting.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
				else if (activity.Improvement != null)
				{
					await activity.Improvement.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
			}

			return new JsonResult(recentActivity);
		}

		public IActionResult WRActivity() => View();

		public async Task<IActionResult> GetWRActivity()
		{
			// Get data
			var recentWRs = await HistoryDAL.GetRecentImprovements(100, rankCutoff: 1);

			// Prepare empty view model
			var recentActivity = new List<Activity>();

			// Add data to view model
			recentWRs.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.UpdatedTimeUTC,
				Improvement = x,
			}));

			// Ordering
			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).ToList();

			// Add avatars
			foreach (var activity in recentActivity)
			{
				if (activity.Sighting != null)
				{
					await activity.Sighting.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
				else if (activity.Improvement != null)
				{
					await activity.Improvement.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
			}

			return new JsonResult(recentActivity);
		}

		public IActionResult Top100Activity() => View();

		public async Task<IActionResult> GetTop100RecentActivity()
		{
			// Get data
			var recentTop100 = await HistoryDAL.GetRecentImprovements(100, rankCutoff: 100);

			// Prepare empty view model
			var recentActivity = new List<Activity>();

			// Add data to view model
			recentTop100.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.UpdatedTimeUTC,
				Improvement = x,
			}));

			// Ordering
			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).ToList();

			// Add avatars
			foreach (var activity in recentActivity)
			{
				if (activity.Sighting != null)
				{
					await activity.Sighting.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
				else if (activity.Improvement != null)
				{
					await activity.Improvement.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
			}

			return new JsonResult(recentActivity);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
