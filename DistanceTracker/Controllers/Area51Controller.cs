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
		public async Task<IActionResult> Index()
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
