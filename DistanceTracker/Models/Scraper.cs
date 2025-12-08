using System;

namespace DistanceTracker.Models
{
	public class Scraper
	{
		public string Name { get; set; }
		public string StatusMessage { get; set; }
		public ulong? LastStartUTC { get; set; }
		public ulong? LastFinishUTC { get; set; }
		public ulong? LastHeartbeatUTC { get; set; }
		public uint APISuccessCount { get; set; }
		public uint APIFailureCount { get; set; }
		public uint LastAPISuccessCount { get; set; }
		public uint LastAPIFailureCount { get; set; }

		public ScraperStatus Status
		{
			get
			{
				if (LastStartUTC == null || LastHeartbeatUTC.Value < ((ulong)DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds()))
				{
					return ScraperStatus.Dead;
				}

				if (LastFinishUTC != null && LastFinishUTC == LastHeartbeatUTC)
				{
					// No errors and also a catch to avoid divide by zero later
					if (LastAPIFailureCount == 0 || LastAPISuccessCount + LastAPIFailureCount == 0)
					{
						return ScraperStatus.Idle;
					}

					if ((decimal)LastAPIFailureCount / (LastAPISuccessCount + LastAPIFailureCount) < (decimal)0.05)
					{
						return ScraperStatus.MinorIssues;
					}
					else
					{
						return ScraperStatus.MajorIssues;
					}
				}

				if (LastHeartbeatUTC >= LastStartUTC)
				{
					// No errors and also a catch to avoid divide by zero later
					if (APIFailureCount == 0 || APISuccessCount + APIFailureCount == 0)
					{
						return ScraperStatus.Running;
					}

					if ((decimal)APIFailureCount / (APISuccessCount + APIFailureCount) < (decimal)0.05)
					{
						return ScraperStatus.MinorIssues;
					}
					else
					{
						return ScraperStatus.MajorIssues;
					}
				}

				return ScraperStatus.Unknown;
			}
		}

		public string StatusString { get { return Status.ToString(); } }
	}
}
