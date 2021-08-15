using System;

namespace DistanceTracker
{
	public class Formatter
	{

		public static string TimeFromMs(ulong milliseconds)
		{
			return TimeFromMs((long)milliseconds);
		}

		public static string TimeFromMs(long milliseconds)
		{
			var seconds = milliseconds / 1000.0;
			var minutes = seconds / 60.0;
			var hours = minutes / 60.0;

			var output = "";
			output += hours >= 1 ? $"{Math.Truncate(hours):0}:" : "";
			output += hours >= 1 || minutes >= 10 ? $"{Math.Truncate(minutes % 60.0):00}:" : minutes >= 1 ? $"{(Math.Truncate(minutes % 60.0)):0}:" : "";
			output += minutes >= 1 || seconds >= 10 ? $"{(Math.Truncate(seconds % 60.0)):00}" : $"{(Math.Truncate(seconds % 60.0)):0}";
			output += $".{(Math.Truncate(milliseconds % 1000.0)):000}s";
			return output;
		}

		public static string TimeAgoFromUnixTime(ulong timestamp)
		{
			var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var diff = currentTimestamp - (long) timestamp;

			var seconds = diff / 1000;
			var minutes = seconds / 60;
			var hours = minutes / 60;
			var days = hours / 24;

			if(days > 0)
			{
				return days > 1 ? $"{days} days ago" : $"{days} day ago";
			}
			else if (hours > 0)
			{
				return hours > 1 ? $"{hours} hours ago" : $"{hours} hour ago";
			}
			else if (minutes > 0)
			{
				return minutes > 1 ? $"{minutes} minutes ago" : $"{minutes} minute ago";
			}
			else
			{
				if(seconds == 0)
				{
					return "Just now";
				}
				return seconds > 1 ? $"{seconds} seconds ago" : $"{seconds} second ago";
			}
		}

		public static string GetOrdinal(int num)
		{
			switch (num % 100)
			{
				case 11:
				case 12:
				case 13:
					return num + "th";
			}

			switch (num % 10)
			{
				case 1:
					return num + "st";
				case 2:
					return num + "nd";
				case 3:
					return num + "rd";
				default:
					return num + "th";
			}
		}
	}
}
