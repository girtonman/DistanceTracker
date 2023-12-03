using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class HistogramViewModel
	{
		public Leaderboard Leaderboard { get; set; }
		public float Percentile { get; set; }
		public ulong Milliseconds { get; set; }
		public List<ulong> BucketKeys { get; set; }
		public List<ulong> BucketCounts { get; set; }
	}
}
