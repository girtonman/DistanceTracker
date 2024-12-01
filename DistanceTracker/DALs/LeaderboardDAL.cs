using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<Leaderboard> GetLeaderboard(uint leaderboardID)
		{
			Connection.Open();

			var sql = $"SELECT ID, LevelName, ImageURL, LeaderboardName, IsOfficial, WorkshopFileID, Tags, LevelType, BronzeMedalTime, SilverMedalTime, GoldMedalTime, DiamondMedalTime FROM Leaderboards WHERE ID = {leaderboardID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			Leaderboard leaderboard = null;
			while (reader.Read())
			{
				var bronzeTime = reader.IsDBNull(8) ? 0 : reader.GetUInt32(8);
				var silverTime = reader.IsDBNull(9) ? 0 : reader.GetUInt32(9);
				var goldTime = reader.IsDBNull(10) ? 0 : reader.GetUInt32(10);
				var diamondTime = reader.IsDBNull(11) ? 0 : reader.GetUInt32(11);

				leaderboard = new Leaderboard()
				{
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					ImageURL = reader.GetString(2),
					LeaderboardName = reader.GetString(3),
					IsOfficial = reader.GetBoolean(4),
					WorkshopFileID = reader.IsDBNull(5) ? (uint?)null : reader.GetUInt32(5),
					Tags = reader.IsDBNull(6) ? "" : reader.GetString(6),
					LevelType = (LevelType)reader.GetUInt32(7),

					MedalTimes = new MapMedalTimes(diamondTime, goldTime, silverTime, bronzeTime),
				};
			}

			Connection.Close();
			return leaderboard;
		}

		public async Task<List<Leaderboard>> GetLeaderboards(bool? isOfficial = null, LevelType? levelType = null)
		{
			Connection.Open();
			var select = "SELECT ID, LevelName, LeaderboardName, IsOfficial FROM Leaderboards ";

			var clauses = new List<string>();
			if(isOfficial.HasValue)
			{
				clauses.Add($"IsOfficial = {(isOfficial.Value ? 1 : 0)}");
			}
			if(levelType.HasValue)
			{
				clauses.Add($"LevelType = {levelType.Value:D}");
			}
			var where = (clauses.Count != 0 ? "WHERE " : "") + string.Join(" AND ", clauses);

			var sql = select + where;
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

			Connection.Close();
			return leaderboards;
		}

		public async Task<List<Leaderboard>> GetAllLeaderboards()
		{
			var leaderboards = new List<Leaderboard>();
			Connection.Open();

			var sql = "SELECT ID, LevelName, LeaderboardName, IsOfficial FROM Leaderboards";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

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

			Connection.Close();
			return leaderboards;
		}

		public async Task<List<Level>> SearchLevels(string search)
		{
			Connection.Open();

			var sql = $@"SELECT
			lb.ID,
			lb.LevelName,
			lb.ImageURL,
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
			{(string.IsNullOrEmpty(search) ? 
				"WHERE lb.IsOfficial = 1 ORDER BY lb.ID" : 
				"WHERE lb.LevelName LIKE @search ORDER BY EntryCount DESC LIMIT 100"
			)}
			";

			var command = new MySqlCommand(sql, Connection);
			if(!string.IsNullOrEmpty(search))
			{
				command.Parameters.AddWithValue("@search", $"%{search}%");
			}
			var reader = await command.ExecuteReaderAsync();

			var levels = new List<Level>();
			while (reader.Read())
			{
				var level = new Level()
				{
					ID = reader.GetUInt32(0),
					LevelName = reader.GetString(1),
					ImageURL = reader.GetString(2),
					LeaderboardName = reader.GetString(3),
					IsOfficial = reader.GetBoolean(4),
					SteamLeaderboardID = reader.IsDBNull(5) ? (ulong?)null : reader.GetUInt64(5),
					LevelSet = reader.IsDBNull(6) ? null : reader.GetString(6),
					EntryCount = reader.IsDBNull(7) ? (uint?)null : reader.GetUInt32(7),
					NewestTimeUTC = reader.IsDBNull(8) ? (ulong?)null : reader.GetUInt64(8),
					NewestImprovementUTC = reader.IsDBNull(9) ? (ulong?)null : reader.GetUInt64(9),
				};
				level.LatestUpdateUTC = level.NewestImprovementUTC.HasValue ?
					(level.NewestTimeUTC.HasValue
						? System.Math.Max(level.NewestTimeUTC.Value, level.NewestImprovementUTC.Value)
						: level.NewestImprovementUTC)
					: level.NewestTimeUTC;

				levels.Add(level);
			}
			reader.Close();
			Connection.Close();

			return levels;
		}

		public async Task UpdateLevelTimes(uint leaderboardID, ulong bronzeTime, ulong silverTime, ulong goldTime, ulong diamondTime)
		{
			Connection.Open();

			var sql = $"UPDATE Leaderboards SET BronzeMedalTime = {bronzeTime}, SilverMedalTime = {silverTime}, GoldMedalTime = {goldTime}, DiamondMedalTime = {diamondTime} WHERE ID = {leaderboardID}";

			var command = new MySqlCommand(sql, Connection);
			await command.ExecuteNonQueryAsync();

			Connection.Close();
		}
	}
}
