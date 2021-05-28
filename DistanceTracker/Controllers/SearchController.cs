using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class SearchController : Controller
	{
		public async Task<IActionResult> Index(string search)
		{
			if(string.IsNullOrEmpty(search))
			{
				return View();
			}

			var dal = new PlayerDAL();
			var players = await dal.SearchByName(search);

			return View(players);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
