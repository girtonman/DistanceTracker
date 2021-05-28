using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class LeaderboardViewModel
	{
		public List<RankedLeaderboardEntry> LeaderboardEntries { get; set; }
		public List<LeaderboardEntry> RecentFirstSightings { get; set; }
		public List<LeaderboardEntryHistory> RecentImprovements { get; set; }
		public Leaderboard Leaderboard { get; set; }
	}
}
