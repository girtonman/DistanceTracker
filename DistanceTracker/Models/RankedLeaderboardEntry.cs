namespace DistanceTracker.Models
{
	public class RankedLeaderboardEntry
	{
		public int Rank { get; set; }
		public double NoodlePoints { get; set; }
		public string NoodlePointsString
		{
			get
			{
				return NoodlePoints.ToString("0");
			}
		}
		public double PlayerRating { get; set; }
		public string PlayerRatingString
		{
			get
			{
				return PlayerRating.ToString("0.00");
			}
		}
		public ulong Milliseconds { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public ulong UpdatedTimeUTC { get; set; }
		public Player Player { get; set; }
		public Leaderboard Leaderboard { get; set; }
	}
}
