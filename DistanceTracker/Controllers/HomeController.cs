using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
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
