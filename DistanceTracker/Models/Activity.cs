namespace DistanceTracker.Models
{
	public class Activity
	{
		public ulong TimeUTC { get; set; }
		public LeaderboardEntry Sighting { get; set; }
		public LeaderboardEntryHistory Improvement { get; set; }
	}
}
