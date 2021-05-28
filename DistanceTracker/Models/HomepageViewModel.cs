using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class HomepageViewModel
	{
		public List<LeaderboardEntryHistory> RecentImprovements { get; set; }
		public List<LeaderboardEntry> RecentFirstSightings { get; set; }
	}
}
