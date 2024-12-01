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

		//public string ConnectionString { get => Configuration["MYSQL_CONNECTION_STRING"]; } // From env
		//public string SteamAPIKey { get => Configuration["STEAM_API_KEY"]; } // From env

		public string ConnectionString { get; } = "server=localhost;uid=website;pwd=password;database=distance"; // Never commit the real connection string to git
		public string SteamAPIKey { get; } = ""; // Fill in with your steam API key (never commit this to git)
	}
}
