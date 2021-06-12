using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class PlayerViewModel
	{
		public double LastWeeksPointsImprovement { get; set; }
		public int LastWeeksRankImprovement { get; set; }
		public double LastWeeksRatingImprovement { get; set; }
		public Player Player { get; set; }
		public RankedLeaderboardEntry GlobalLeaderboardEntry { get; set; }
		public List<Activity> RecentActivity { get; set; }
		public List<RankedLeaderboardEntry> RankedLeaderboardEntries { get; set; }
	}
}
