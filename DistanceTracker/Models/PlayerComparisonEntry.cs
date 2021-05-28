namespace DistanceTracker.Models
{
	public class PlayerComparisonEntry
	{
		public Leaderboard Leaderboard { get; set; }
		public RankedLeaderboardEntry LeftEntry { get; set; }
		public RankedLeaderboardEntry RightEntry { get; set; }
	}
}
