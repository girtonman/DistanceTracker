namespace DistanceTracker.Models
{
	public class Activity
	{
		public ulong TimeUTC { get; set; }
		public string TimeAgoString
		{
			get
			{
				return Formatter.TimeAgoFromUnixTime(TimeUTC);
			}
		}
		public LeaderboardEntry Sighting { get; set; }
		public LeaderboardEntryHistory Improvement { get; set; }
	}
}
