namespace DistanceTracker.Models
{
	public class PercentileRank
	{
		public ulong Milliseconds { get; set; }
		public float Percentile { get; set; }
		public uint LeaderboardID { get; set; }
		public string LevelName { get; set; }
		public string LeaderboardName { get; set; }
		public string LevelSet { get; set; }
	}
}
