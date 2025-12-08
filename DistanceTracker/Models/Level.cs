namespace DistanceTracker.Models
{
	public class Level
	{
		public uint ID { get; set; }
		public string LevelName { get; set; }
		public string ImageURL { get; set; }
		public string LeaderboardName { get; set; }
		public bool IsOfficial { get; set; }
		public ulong? SteamLeaderboardID { get; set; }
		public string LevelSet { get; set; }
		public uint? EntryCount { get; set; }
		public ulong? NewestTimeUTC { get; set; }
		public ulong? NewestImprovementUTC { get; set; }
		public ulong? LatestUpdateUTC { get; set; }
		public ulong? LastScannedTimeUTC { get; set; }
		public string UpdateTimeAgoString
		{
			get
			{
				return LatestUpdateUTC.HasValue ? Formatter.TimeAgoFromUnixTime(LatestUpdateUTC.Value) : "";
			}
		}
		public string ScanTimeAgoString
		{
			get
			{
				return LastScannedTimeUTC.HasValue ? Formatter.TimeAgoFromUnixTime(LastScannedTimeUTC.Value) : "";
			}
		}
		
	}
}
