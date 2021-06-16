using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class PlayerComparisonEntry
	{
		public Leaderboard Leaderboard { get; set; }
		public Dictionary<ulong, RankedLeaderboardEntry> RankedEntries { get; set; }
	}
}
