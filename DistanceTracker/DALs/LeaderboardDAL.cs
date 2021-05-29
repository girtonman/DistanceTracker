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

		public async Task<List<Level>> GetLevels()
        {
			Connection.Open();

			var sql = @"SELECT
			lb.*,
			LeaderboardCounts.EntryCount,
			RecentFirstSightings.NewestTimeUTC,
			RecentImprovements.NewestImprovementUTC
			FROM Leaderboards lb
			LEFT JOIN (
			    SELECT LeaderboardID, COUNT(*) AS EntryCount FROM LeaderboardEntries
			    GROUP BY LeaderboardID
			) LeaderboardCounts ON lb.ID = LeaderboardCounts.LeaderboardID
			LEFT JOIN (
			    SELECT 
			    lbe.LeaderboardID,
			    MAX(lbe.FirstSeenTimeUTC) AS NewestTimeUTC
			    FROM LeaderboardEntries AS lbe
			    GROUP BY lbe.LeaderboardID
			) RecentFirstSightings ON lb.ID = RecentFirstSightings.LeaderboardID
			LEFT JOIN (
			    SELECT 
			    lbeh.LeaderboardID,
			    MAX(lbeh.UpdatedTimeUTC) AS NewestImprovementUTC
			    FROM LeaderboardEntryHistory AS lbeh
			    GROUP BY lbeh.LeaderboardID
			) RecentImprovements ON lb.ID = RecentImprovements.LeaderboardID
			";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var levels = new List<Level>();
			while(reader.Read())
            {
				levels.Add(new Level()
                {
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					LeaderboardName = reader.GetString(2),
					IsOfficial = reader.GetBoolean(3),
					SteamLeaderboardID = reader.GetUInt32(4),
					EntryCount = reader.GetUInt32(5),
					NewestTimeUTC = reader.GetUInt32(6),
					NewestImprovementUTC = reader.GetUInt32(7),
                });
            }
			reader.Close();
			Connection.Close();

			return levels;
        }
	}
}
