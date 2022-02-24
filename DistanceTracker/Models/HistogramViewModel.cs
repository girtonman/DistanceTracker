using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
