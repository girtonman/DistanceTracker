namespace DistanceTracker.Models
{
	public class Leaderboard
	{
		public uint ID { get; set; }
		public string LevelName { get; set; }
		public string ImageURL { get; set; }
		public string LeaderboardName { get; set; }
		public string LevelSet { get; set; }
		public bool IsOfficial { get; set; }
		public uint? WorkshopFileID { get; set; }
		public string Tags { get; set; }
		public LevelType LevelType { get; set; }
		public bool IsReverseRankOrder { get => LevelType == LevelType.Stunt; }
		public MapMedalTimes MedalTimes { get; set; }
	}
}
