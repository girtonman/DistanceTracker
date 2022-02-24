namespace DistanceTracker.Models
{
	public class HistogramDataPoint
	{
		public ulong BucketFloor { get; set; }
		public ulong BucketCount { get; set; }
		public uint LeaderboardID { get; set; }
	}
}
