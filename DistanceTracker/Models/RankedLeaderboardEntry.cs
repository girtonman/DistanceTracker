namespace DistanceTracker
{
	public class RankedLeaderboardEntry
	{
		public int Rank { get; set; }
		public double NoodlePoints { get; set; }
		public double PlayerRating { get; set; }
		public ulong Milliseconds { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public Player Player { get; set; }
		public Leaderboard Leaderboard { get; set; }
	}
}
