namespace DistanceTracker.Models
{
	public class Event
	{
		public uint ID { get; set; }
		public string Name { get; set; }
		public ulong? StartTimeUTC { get; set; }
		public ulong? EndTimeUTC { get; set; }
		public string EventBackgroundImageURL { get; set; }
	}
}
