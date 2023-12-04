namespace DistanceTracker.Models
{
	public class GlobalRankedLeaderboardEntry : RankedLeaderboardEntry
	{
		public ulong TotalMilliseconds { get; set; }
		public uint NumTracksCompleted { get; set; }

		public long LastWeeksGlobalTimeImprovement { get; set; }
	}
}
