using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class SearchController : Controller
	{
		public SearchController(PlayerDAL playerDAL, SteamDAL steamDAL)
		{
			PlayerDAL = playerDAL;
			SteamDAL = steamDAL;
		}

		public PlayerDAL PlayerDAL { get; }
		public SteamDAL SteamDAL { get; }

		public IActionResult Index(string q) => View("Index", q);

		public async Task<IActionResult> Players(string q)
		{
			if (string.IsNullOrEmpty(q))
			{
				return new JsonResult(new List<Player>());
			}

			var players = await PlayerDAL.SearchByName(q);
			foreach (var player in players)
			{
				await player.GetSteamAvatar(SteamDAL, PlayerDAL);
			}

			return new JsonResult(players);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
