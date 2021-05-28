using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class PlayerComparisonViewModel
	{
		public List<PlayerComparisonEntry> Comparisons { get; set; }
		public Player LeftPlayer { get; set; }
		public Player RightPlayer { get; set; }
	}
}
