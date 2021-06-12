using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class SearchController : Controller
	{
		public async Task<IActionResult> Index(string q)
		{
			if(string.IsNullOrEmpty(q))
			{
				return View();
			}

			var dal = new PlayerDAL();
			var players = await dal.SearchByName(q);

			return View(players);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
