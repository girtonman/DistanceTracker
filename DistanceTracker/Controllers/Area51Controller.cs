using DistanceTracker.DALs;
using DistanceTracker.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class Area51Controller : Controller
	{
		public Area51Controller(SteamDAL steamDAL, LeaderboardDAL leaderboardDAL)
		{
			SteamDAL = steamDAL;
			LeaderboadDAL = leaderboardDAL;
		}

		public SteamDAL SteamDAL { get; }
		public LeaderboardDAL LeaderboadDAL { get; }

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

		public async Task<IActionResult> PopulateOfficialTimes()
		{
			foreach (var medalTimes in OfficialMapMedalTimes.Maps)
			{
				await LeaderboadDAL.UpdateLevelTimes(medalTimes.Key, medalTimes.Value.BronzeTime, medalTimes.Value.SilverTime, medalTimes.Value.GoldTime, medalTimes.Value.DiamondTime);
			}

			return Json("Done");
		}
	}
}
