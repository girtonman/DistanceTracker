using System.Collections.Generic;

namespace DistanceTracker.Models
{
	public class PlayerComparisonViewModel
	{
		public List<PlayerComparisonEntry> Comparisons { get; set; }
		public List<Player> Players { get; set; }
		public string SteamIDs { get; set; }
	}
}
