using System;

namespace DistanceTracker
{
	public class Formatter
	{
		public static string TimeFromMs(ulong milliseconds)
		{
			var seconds = milliseconds / 1000;
			var minutes = seconds / 60;
			var hours = minutes / 60;

			var output = "";
			output += hours == 0 ? "" : $"{hours.ToString("00")}:";
			output += minutes == 0 ? "" : $"{(minutes%60).ToString("00")}:";
			output += $"{((milliseconds / 1000.0) % 60).ToString("00.000")}s";
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
	}
}
