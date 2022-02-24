namespace DistanceTracker.Models
{
	public class FunStats
	{
		public int TracksCompleted { get; set; }
		public int TotalImprovements { get; set; }
		public ulong FirstSeenTimeUTC { get; set; }
		public string FirstSeen
		{
			get
			{
				return Formatter.TimeAgoFromUnixTime(FirstSeenTimeUTC);
			}
		}
		public int MostImprovements { get; set; }
		public string MostImprovementsLevel { get; set; }
	}
}
