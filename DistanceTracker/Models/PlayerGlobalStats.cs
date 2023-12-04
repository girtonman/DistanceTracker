namespace DistanceTracker.Models
{
	public class PlayerGlobalStats
	{
		public double LastWeeksPointsImprovement { get; set; }
		public int LastWeeksRankImprovement { get; set; }
		public double LastWeeksRatingImprovement { get; set; }
		public RankedLeaderboardEntry GlobalLeaderboardEntry { get; set; }
		public FunStats FunStats { get; set; }
	}
}
