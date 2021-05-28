using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardDAL()
		{
			Connection = new MySqlConnection(Settings.ConnectionString);
		}
		public async Task<Leaderboard> GetLeaderboard(uint leaderboardID)
		{
			Connection.Open();

			var sql = $"SELECT ID, LevelName, LeaderboardName, IsOfficial FROM Leaderboards WHERE ID = {leaderboardID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			Leaderboard leaderboard = null;
			while (reader.Read())
			{
				leaderboard = new Leaderboard()
				{
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					LeaderboardName = reader.GetString(2),
					IsOfficial = reader.GetBoolean(3),
				};
			}

			Connection.Close();
			return leaderboard;
		}

		public async Task<List<Leaderboard>> GetAllLeaderboards()
		{
			var leaderboards = new List<Leaderboard>();
			Connection.Open();

			var sql = "SELECT ID, LevelName, LeaderboardName, IsOfficial FROM Leaderboards";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			while(reader.Read())
			{
				leaderboards.Add(new Leaderboard() {
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					LeaderboardName = reader.GetString(2),
					IsOfficial = reader.GetBoolean(3),
				});
			}

			Connection.Close();
			return leaderboards;
		}

		public async Task<List<Leaderboard>> GetOfficialSprintLeaderboards()
		{
			Connection.Open();

			var sql = "SELECT ID, LevelName, LeaderboardName, IsOfficial FROM Leaderboards WHERE IsOfficial = true";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var leaderboards = new List<Leaderboard>();
			while (reader.Read())
			{
				leaderboards.Add(new Leaderboard()
				{
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					LeaderboardName = reader.GetString(2),
					IsOfficial = reader.GetBoolean(3),
				});
			}
			reader.Close();
			Connection.Close();

			return leaderboards;
		}
	}
}
