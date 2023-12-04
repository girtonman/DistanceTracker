namespace DistanceTracker.Models
{
	public class MapMedalTimes
	{
		public ulong DiamondTime { get; set; }
		public ulong GoldTime { get; set; }
		public ulong SilverTime { get; set; }
		public ulong BronzeTime { get; set; }

		public MapMedalTimes(ulong diamondMilliseconds, ulong goldMilliseconds, ulong silverMilliseconds, ulong bronzeMilliseconds)
		{
			DiamondTime = diamondMilliseconds;
			GoldTime = goldMilliseconds;
			SilverTime = silverMilliseconds;
			BronzeTime = bronzeMilliseconds;
		}

		public string GetMedalCSS(ulong milliseconds)
		{
			if(milliseconds < DiamondTime)
			{
				return "medal-diamond";
			}
			if(milliseconds < GoldTime)
			{
				return "medal-gold";
			}
			if(milliseconds < SilverTime)
			{
				return "medal-silver";
			}
			return "medal-bronze";
		}
	}
}
