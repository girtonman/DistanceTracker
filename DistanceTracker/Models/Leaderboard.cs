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
		public MapMedalTimes MedalTimes { get; set; }
	}
}
