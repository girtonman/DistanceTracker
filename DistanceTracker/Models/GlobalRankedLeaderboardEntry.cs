namespace DistanceTracker.Models
{
	public class OverviewRankedLeaderboardEntry : RankedLeaderboardEntry
	{
		public ulong TotalMilliseconds { get; set; }
		public ulong TotalStuntScore { get; set; }
		public uint NumTracksCompleted { get; set; }

		public long LastWeeksTimeImprovement { get; set; }
		public long LastWeeksScoreImprovement { get; set; }
	}
}
