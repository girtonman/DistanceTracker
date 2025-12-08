namespace DistanceTracker.Models
{
	public class RequestedUpdate
	{
		public uint ID { get; set; }
		public uint LeaderboardID { get; set; }
		public ulong RequestedTimeUTC { get; set; }
		public ulong? ProcessedTimeUTC { get; set; }
	}
}
