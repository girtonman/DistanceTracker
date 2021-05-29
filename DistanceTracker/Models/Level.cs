namespace DistanceTracker
{
	public class Level
	{
		public uint ID { get; set; }
		public string LevelName { get; set; }
		public string LeaderboardName { get; set; }
		public bool IsOfficial { get; set; }
		public uint SteamLeaderboardID { get; set; }
		public uint EntryCount { get; set; }
		public uint NewestTimeUTC { get; set; }
		public uint NewestImprovementUTC { get; set; }
	}
}
