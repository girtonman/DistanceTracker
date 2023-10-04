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
		public Area51Controller(SteamDAL steamDAL)
		{
			SteamDAL = steamDAL;
		}

		public SteamDAL SteamDAL { get; }

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
			var players = await SteamDAL.GetPlayerSummaries(steamID);

			return View(players.FirstOrDefault());
		}
	}
}
