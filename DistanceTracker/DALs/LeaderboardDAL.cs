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

		public async Task<List<Level>> GetLevels()
		{
			Connection.Open();

			var sql = @"SELECT
			lb.ID,
			lb.LevelName,
			lb.LeaderboardName,
			lb.IsOfficial,
			lb.SteamLeaderboardID,
			lb.LevelSet,
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
				var level = new Level()
				{
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					LeaderboardName = reader.GetString(2),
					IsOfficial = reader.GetBoolean(3),
					SteamLeaderboardID = reader.GetUInt64(4),
					LevelSet = reader.IsDBNull(5) ? null : reader.GetString(5),
					EntryCount = reader.IsDBNull(6) ? (uint?)null : reader.GetUInt32(6),
					NewestTimeUTC = reader.IsDBNull(7) ? (ulong?)null : reader.GetUInt64(7),
					NewestImprovementUTC = reader.IsDBNull(8) ? (ulong?)null : reader.GetUInt64(8),
				};
				level.LatestUpdateUTC = level.NewestImprovementUTC.HasValue ? 
					(level.NewestTimeUTC.HasValue 
						? System.Math.Max(level.NewestTimeUTC.Value, level.NewestImprovementUTC.Value) 
						: level.NewestTimeUTC) 
					: null;

				levels.Add(level);
			}
			reader.Close();
			Connection.Close();

			return levels;
		}
	}
}
