using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class GeneralDAL
	{
		private MySqlConnection Connection { get; set; }

		public GeneralDAL()
		{
			Connection = new MySqlConnection(Settings.ConnectionString);
		}

		public async Task<SiteStats> GetSiteStats()
		{
			Connection.Open();

			var sql = @"
			SELECT
				(SELECT COUNT(*) FROM Players) PlayerCount,
				(SELECT COUNT(*) FROM LeaderboardEntries) LeaderboardEntryCount,
				(SELECT COUNT(*) FROM LeaderboardEntryHistory) ImprovementCount,
				(SELECT COUNT(*) FROM Leaderboards) LeaderboardCount
			FROM (SELECT 1) dummy
			";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var siteStats = new SiteStats();
			while(reader.Read())
			{
				siteStats.PlayerCount = reader.GetInt32(0);
				siteStats.LeaderboardEntryCount = reader.GetInt32(1);
				siteStats.ImprovementCount = reader.GetInt32(2);
				siteStats.LeaderboardCount = reader.GetInt32(3);
			}
			reader.Close();
			Connection.Close();

			return siteStats;
		}
	}
}
