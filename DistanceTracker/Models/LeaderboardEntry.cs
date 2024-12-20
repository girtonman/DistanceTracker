﻿namespace DistanceTracker.Models
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
				return Leaderboard.LevelType == LevelType.Stunt ? Formatter.ElectronVolts(Milliseconds) : Formatter.TimeFromMs(Milliseconds);
			}
		}
		public ulong SteamID { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public ulong UpdatedTimeUTC { get; set; }
		public Leaderboard Leaderboard { get; set; }
		public Player Player { get; set; }
		public string UpdatedTimeAgoString { get => Formatter.TimeAgoFromUnixTime(UpdatedTimeUTC); }
	}
}
