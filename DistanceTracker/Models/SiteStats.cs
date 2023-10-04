namespace DistanceTracker.Models
{
	public class SiteStats
	{
		public int PlayerCount { get; set; }
		public int LeaderboardEntryCount { get; set; }
		public int ImprovementCount { get; set; }
		public int LeaderboardCount { get; set; }
		public ulong UpdatedTimeUTC { get; set; }
		public string TimeAgoString
		{
			get
			{
				return Formatter.TimeAgoFromUnixTime(UpdatedTimeUTC);
			}
		}
	}
}
