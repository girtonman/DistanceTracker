using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class ScraperDAL
	{
		private MySqlConnection Connection { get; set; }

		public ScraperDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<List<Scraper>> GetAllScrapers()
		{
			Connection.Open();
			var sql = @"
				SELECT
					Name,
					StatusMessage,
					LastStartUTC,
					LastFinishUTC,
					LastHeartbeatUTC,
					APISuccessCount,
					APIFailureCount,
					LastAPISuccessCount,
					LastAPIFailureCount
				FROM Scrapers;
			";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var scrapers = new List<Scraper>();
			while (reader.Read())
			{
				scrapers.Add(new Scraper()
				{
					Name = reader.GetString(0),
					StatusMessage = reader.GetString(1),
					LastStartUTC = reader.IsDBNull(2) ? (ulong?)null : reader.GetUInt64(2),
					LastFinishUTC = reader.IsDBNull(3) ? (ulong?)null : reader.GetUInt64(3),
					LastHeartbeatUTC = reader.IsDBNull(4) ? (ulong?)null : reader.GetUInt64(4),
					APISuccessCount = reader.IsDBNull(5) ? 0 : reader.GetUInt32(5),
					APIFailureCount = reader.IsDBNull(6) ? 0 : reader.GetUInt32(6),
					LastAPISuccessCount = reader.IsDBNull(7) ? 0 : reader.GetUInt32(7),
					LastAPIFailureCount = reader.IsDBNull(8) ? 0 : reader.GetUInt32(8),
				});
			}
			reader.Close();
			Connection.Close();

			return scrapers;
		}
	}
}
