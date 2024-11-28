namespace DistanceTracker.Models
{
	public class LeaderboardEntryHistory
	{
		public uint ID { get; set; }
		public uint LeaderboardID { get; set; }
		public ulong SteamID { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public ulong OldMilliseconds { get; set; }
		public ulong NewMilliseconds { get; set; }
		public string MillisecondsString
		{
			get
			{
				return Leaderboard.LevelType == LevelType.Stunt ? Formatter.ElectronVolts(NewMilliseconds) : Formatter.TimeFromMs(NewMilliseconds);
			}
		}
		public string TimeImprovement
		{
			get
			{
				return Leaderboard.LevelType == LevelType.Stunt ? Formatter.ElectronVolts(NewMilliseconds - OldMilliseconds) : Formatter.TimeFromMs(OldMilliseconds - NewMilliseconds);
			}
		}
		public uint OldRank { get; set; }
		public uint NewRank { get; set; }
		public ulong UpdatedTimeUTC { get; set; }
		public Leaderboard Leaderboard { get; set; }
		public Player Player { get; set; }
		public string TimeAgoString
		{
			get
			{
				return Formatter.TimeAgoFromUnixTime(UpdatedTimeUTC);
			}
		}
	}
}
