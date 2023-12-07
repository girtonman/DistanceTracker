namespace DistanceTracker.Models
{
	public class OverviewRankedLeaderboardEntry : RankedLeaderboardEntry
	{
		public ulong TotalMilliseconds { get; set; }
		public uint NumTracksCompleted { get; set; }

		public long LastWeeksTimeImprovement { get; set; }
	}
}
