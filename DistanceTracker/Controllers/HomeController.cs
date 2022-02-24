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
		public async Task<IActionResult> IndexAsync()
		{
			var dal = new GeneralDAL();
			var siteStats = await dal.GetSiteStats();
			return View(siteStats);
		}

		public IActionResult GlobalActivity() => View();

		public async Task<IActionResult> GetGlobalRecentActivity()
		{
			// Get data
			var leDAL = new LeaderboardEntryDAL();
			var lehDAL = new LeaderboardEntryHistoryDAL();
			var recentFirstSightings = await leDAL.GetRecentFirstSightings(100);
			var recentImprovements = await lehDAL.GetRecentImprovements(100);

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
					await activity.Sighting.Player.GetSteamAvatar();
				}
				else if (activity.Improvement != null)
				{
					await activity.Improvement.Player.GetSteamAvatar();
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
