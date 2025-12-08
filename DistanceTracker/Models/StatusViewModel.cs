using System.Collections.Generic;
using System.Linq;

namespace DistanceTracker.Models
{
	public class StatusViewModel
	{
		// Keep some info about the overall status at the top level of the viewmodel
		// Allows for special formatting above the table of scrapers
		public ScraperStatus OverallStatus { get; set; }
		public Scraper MainScraper { get { return Scrapers.FirstOrDefault(x => x.Name == "Scraper"); } }
		public string StatusString
		{
			get
			{
				if (MainScraper.LastStartUTC == null)
				{
					return "Never started";
				}

				if (MainScraper.LastFinishUTC == null || MainScraper.LastStartUTC > MainScraper.LastFinishUTC)
				{
					return $"Started {Formatter.TimeAgoFromUnixTime(MainScraper.LastStartUTC.Value)}";
				}
				else
				{
					return $"Completely died {Formatter.TimeAgoFromUnixTime(MainScraper.LastFinishUTC.Value)}";
				}
			}
		}

		// All the scrapers that will be rendered in a list
		public List<Scraper> Scrapers { get; set; }

		public ulong LastCompleteScanUTC { get; set; }
	}
}
