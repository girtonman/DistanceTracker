using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class OverviewLeaderboardViewModel
	{
		public string LeaderboardName { get; set; }
		public bool HasTimeScores { get; set; }
		public bool HasStuntScores { get; set; }
		public List<WinnersCircleEntry> WinnersCircle { get; set; }
		public List<OverviewRankedLeaderboardEntry> LeaderboardEntries { get; set; }
		public List<LeaderboardEntryHistory> WRLog { get; set; }
		public ulong OptimalTotalTime { get; set; }
		public ulong OptimalTotalStuntScore { get; set; }
		public ulong LastWeeksTimeImprovement { get; set; }
		public Event EventDetails { get; set; }
		public Player FirstPlace { get => LeaderboardEntries.Count > 0 ? LeaderboardEntries[0].Player : null; }
		public Player SecondPlace { get => LeaderboardEntries.Count > 1 ? LeaderboardEntries[1].Player : null; }
		public Player ThirdPlace { get => LeaderboardEntries.Count > 2 ? LeaderboardEntries[2].Player : null; }

	}
}
