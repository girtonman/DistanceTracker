using Microsoft.Extensions.Configuration;

namespace DistanceTracker
{
	public class Settings
	{
		public Settings(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public string ConnectionString { get => Configuration["ConnectionString"]; }
		public string SteamAPIKey { get => Configuration["SteamAPIKey"]; }
	}
}
