using System;
using System.Collections.Generic;
using System.Linq;

namespace DistanceTracker
{
	public class NoodlePointsUtil
	{
		public static readonly int NUM_OFFICIAL_SPRINTS = 99;
		public static readonly int MAX_POINTS_PER_MAP = 1000;
		public static readonly int RANK_CUTOFF = 1000;

		public static int CalculateImprovement(List<LeaderboardEntryHistory> improvements)
		{
			return improvements.Sum(x => CalculateDifference((int) x.OldRank, (int) x.NewRank));
		}

		public static int CalculateDifference(int oldRank, int newRank)
		{
			return CalculatePoints(newRank) - CalculatePoints(oldRank);
		}

		public static int CalculatePoints(int rank)
		{
			// C# version of the SQL point calculation formula
			//ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2))))
			if(rank > 1000)
			{
				return 0;
			}

			return (int) (1000.0 * (1.0 - Math.Sqrt(1.0 - Math.Pow(((rank - 1.0) / 1000.0) - 1.0, 2))));
		}
	}
}
