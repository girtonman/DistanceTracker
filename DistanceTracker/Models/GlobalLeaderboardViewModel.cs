using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.Models
{
	public class GlobalLeaderboardViewModel
	{
		public List<WinnersCircleEntry> WinnersCircle { get; set; }
		public List<RankedLeaderboardEntry> LeaderboardEntries { get; set; }
	}
}
