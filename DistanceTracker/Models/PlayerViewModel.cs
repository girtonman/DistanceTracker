using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class PlayerViewModel
	{
		public string SteamProfilePicURL { get; set; }
		public double LastDaysPointsImprovement { get; set; }
		public int LastDaysRankImprovement { get; set; }
		public double LastDaysRatingImprovement { get; set; }
		public Player Player { get; set; }
		public RankedLeaderboardEntry GlobalLeaderboardEntry { get; set; }
		public List<Activity> RecentActivity { get; set; }
		public List<RankedLeaderboardEntry> RankedLeaderboardEntries { get; set; }
	}
}
