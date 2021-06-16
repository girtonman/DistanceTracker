using DistanceTracker.DALs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class Area51Controller : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Medals()
		{
			return View();
		}

		public async Task<IActionResult> SteamAPITest(ulong steamID)
		{
			var api = new SteamDAL();
			var players = await api.GetPlayerSummaries(steamID);

			return View(players.FirstOrDefault());
		}
	}
}
