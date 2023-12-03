using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class GlobalLeaderboardViewModel
	{
		public List<WinnersCircleEntry> WinnersCircle { get; set; }
		public List<GlobalRankedLeaderboardEntry> LeaderboardEntries { get; set; }
		public List<LeaderboardEntryHistory> WRLog { get; set; }
		public ulong OptimalTotalTime { get; set; }
		public ulong LastWeeksTimeImprovement { get; set; }
	}
}
