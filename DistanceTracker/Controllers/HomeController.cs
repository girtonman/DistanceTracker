using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
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

		public async Task<IActionResult> GlobalActivity()
		{
			var lehDAL = new LeaderboardEntryHistoryDAL();
			var leDAL = new LeaderboardEntryDAL();
			var viewModel = new HomepageViewModel
			{
				RecentImprovements = await lehDAL.GetRecentImprovements(100),
				RecentFirstSightings = await leDAL.GetRecentFirstSightings(100)
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
