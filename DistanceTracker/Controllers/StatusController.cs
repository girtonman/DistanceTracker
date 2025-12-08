using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class StatusController : Controller
	{
		public StatusController(ScraperDAL scraperDAL, LeaderboardDAL leaderboardDAL)
		{
			ScraperDAL = scraperDAL;
			LeaderboardDAL = leaderboardDAL;
		}

		public ScraperDAL ScraperDAL { get; }
		public LeaderboardDAL LeaderboardDAL { get; }

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> GetScraperStatus(ulong steamID)
		{
			var viewModel = new StatusViewModel
			{
				Scrapers = await ScraperDAL.GetAllScrapers(),
				LastCompleteScanUTC = await LeaderboardDAL.GetOldestScanTime(),
			};

			return new JsonResult(viewModel);
		}
	}
}
