namespace DistanceTracker
{
	public class LeaderboardEntry
	{
		public uint ID { get; set; }
		public uint LeaderboardID { get; set; }
		public ulong Milliseconds { get; set; }
		public string MillisecondsString
		{
			get
			{
				return Formatter.TimeFromMs(Milliseconds);
			}
		}
		public ulong SteamID { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public Leaderboard Leaderboard { get; set; }
		public Player Player { get; set; }
	}
}
