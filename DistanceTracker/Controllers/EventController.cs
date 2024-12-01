using DistanceTracker.DALs;
using DistanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Controllers
{
	public class EventController : Controller
	{
		public EventController(GeneralDAL generalDAL, LeaderboardEntryDAL leDAL, LeaderboardEntryHistoryDAL lehDAL, SteamDAL steamDAL, PlayerDAL playerDAL, LeaderboardDAL leaderboardDAL, EventDAL eventDAL)
		{
			GeneralDAL = generalDAL;
			EntryDAL = leDAL;
			HistoryDAL = lehDAL;
			SteamDAL = steamDAL;
			PlayerDAL = playerDAL;
			LeaderboardDAL = leaderboardDAL;
			EventDAL = eventDAL;
		}

		public GeneralDAL GeneralDAL { get; }
		public LeaderboardEntryDAL EntryDAL { get; }
		public LeaderboardEntryHistoryDAL HistoryDAL { get; }
		public SteamDAL SteamDAL { get; }
		public PlayerDAL PlayerDAL { get; }
		public LeaderboardDAL LeaderboardDAL { get; }
		public EventDAL EventDAL { get; }

		public async Task<IActionResult> GetRecentEventActivity(uint eventID, ulong? after = null)
		{
			// Get leaderboards for the event
			var eventLeaderboardIDs = await EventDAL.GetEventLeaderboards(eventID);

			// Get data
			var recentFirstSightings = await EntryDAL.GetRecentFirstSightings(numRows: 100, after: after, leaderboardIDs: eventLeaderboardIDs);
			var recentImprovements = await HistoryDAL.GetRecentImprovements(numRows: 100, after: after, leaderboardIDs: eventLeaderboardIDs);

			// Prepare empty view model
			var recentActivity = new List<Activity>();

			// Add data to view model
			recentFirstSightings.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.FirstSeenTimeUTC,
				Sighting = x,
			}));
			recentImprovements.ForEach(x => recentActivity.Add(new Activity()
			{
				TimeUTC = x.UpdatedTimeUTC,
				Improvement = x,
			}));

			// Ordering
			recentActivity = recentActivity.OrderByDescending(x => x.TimeUTC).Take(100).ToList();

			// Add avatars
			foreach (var activity in recentActivity)
			{
				if (activity.Sighting != null)
				{
					await activity.Sighting.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
				else if (activity.Improvement != null)
				{
					await activity.Improvement.Player.GetSteamAvatar(SteamDAL, PlayerDAL);
				}
			}

			return new JsonResult(recentActivity);
		}

		public async Task<IActionResult> Overview(uint eventID)
		{
			// Get leaderboards for the event
			var eventLeaderboardIDs = await EventDAL.GetEventLeaderboards(eventID);
			var eventDetails = await EventDAL.GetEventDetails(eventID);
			var maxEntryCount = await EntryDAL.GetMaxEntryCount(eventLeaderboardIDs);

			var viewModel = new OverviewLeaderboardViewModel
			{
				LeaderboardEntries = await EntryDAL.GetMultiLevelLeaderboard(maxEntryCount, eventLeaderboardIDs),
				WinnersCircle = await EntryDAL.GetMultiLevelWinnersCircle(eventLeaderboardIDs),
				OptimalTotalTime = await EntryDAL.GetOptimalTotal(eventLeaderboardIDs),
				OptimalTotalStuntScore = await EntryDAL.GetOptimalTotal(eventLeaderboardIDs, true),
				WRLog = await HistoryDAL.GetWRLog(leaderboardIDs: eventLeaderboardIDs),
				EventDetails = eventDetails,
				HasStuntScores = true,
				HasTimeScores = true,
			};

			// Add global time improvements to the entries
			var globalTimeImprovements = await HistoryDAL.GetPastWeeksImprovement(leaderboardIDs: eventLeaderboardIDs);
			foreach (var entry in viewModel.LeaderboardEntries)
			{
				var steamID = entry.Player.SteamID;
				if (globalTimeImprovements.ContainsKey(steamID))
				{
					entry.LastWeeksTimeImprovement = globalTimeImprovements[steamID].Item1;
					entry.LastWeeksScoreImprovement = globalTimeImprovements[steamID].Item2;
				}
			}

			return View(viewModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
