using DistanceTracker.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardEntryHistoryDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardEntryHistoryDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<List<LeaderboardEntryHistory>> GetRecentImprovements(int numRows = 20, ulong? steamID = null, List<uint> leaderboardIDs = null, uint? rankCutoff = null, ulong? after = null)
		{
			Connection.Open();
			// Construct the base SELECT
			var sql = @$"
				SELECT
					leh.LeaderboardID,
					l.LevelName,
					leh.SteamID,
					p.Name,
					leh.FirstSeenTimeUTC,
					OldMilliseconds,
					NewMilliseconds,
					OldRank,
					NewRank,
					UpdatedTimeUTC,
					p.SteamAvatar,
					l.ImageURL,
					l.LevelType
				FROM LeaderboardEntryHistory leh
				LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID
				LEFT JOIN Players p on p.SteamID = leh.SteamID";

			// Add conditions
			var conditions = new List<string>();
			if (steamID.HasValue)
			{
				conditions.Add($"leh.SteamID = {steamID}");
			}
			if (leaderboardIDs != null && leaderboardIDs.Count > 0)
			{
				conditions.Add($"leh.LeaderboardID IN ({string.Join(",", leaderboardIDs)})");
			}
			if (rankCutoff.HasValue)
			{
				conditions.Add($"leh.NewRank <= {rankCutoff}");
			}
			if (after.HasValue)
			{
				conditions.Add($"leh.UpdatedTimeUTC > {after}");
			}

			for (var i = 0; i < conditions.Count; i++)
			{
				if (i == 0)
				{
					sql += $" WHERE {conditions[i]}";
				}
				else
				{
					sql += $" AND {conditions[i]}";
				}
			}

			// Add ordering
			sql += $" ORDER BY leh.ID DESC LIMIT {numRows}";

			// Execute and handle result
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();
			var entries = new List<LeaderboardEntryHistory>();
			while (reader.Read())
			{
				var leh = new LeaderboardEntryHistory()
				{
					LeaderboardID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(2),
					FirstSeenTimeUTC = reader.GetUInt64(4),
					OldMilliseconds = reader.GetUInt64(5),
					NewMilliseconds = reader.GetUInt64(6),
					OldRank = reader.GetUInt32(7),
					NewRank = reader.GetUInt32(8),
					UpdatedTimeUTC = reader.GetUInt64(9),
				};
				leh.Leaderboard = new Leaderboard()
				{
					ID = leh.LeaderboardID,
					LevelName = reader.GetString(1),
					ImageURL = reader.GetString(11),
					LevelType = (LevelType)reader.GetUInt32(12),
				};
				leh.Player = new Player()
				{
					SteamID = leh.SteamID,
					Name = reader.GetString(3),
					SteamAvatar = reader.IsDBNull(10) ? null : reader.GetString(10),
				};
				entries.Add(leh);
			}
			reader.Close();
			Connection.Close();

			return entries;
		}

		public async Task<List<LeaderboardEntryHistory>> GetPastWeeksImprovements(ulong steamID, List<uint> leaderboardIDs)
		{
			Connection.Open();
			var sql = @$"
				SELECT 
					leh.LeaderboardID,
					l.LevelName,
					leh.SteamID,
					p.Name,
					leh.FirstSeenTimeUTC,
					OldMilliseconds,
					NewMilliseconds,
					OldRank,
					NewRank,
					UpdatedTimeUTC,
					p.SteamAvatar
				FROM LeaderboardEntryHistory leh 
				LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID 
				LEFT JOIN Players p on p.SteamID = leh.SteamID 
				WHERE leh.SteamID = {steamID} AND l.ID IN ({string.Join(",", leaderboardIDs)})
					AND leh.UpdatedTimeUTC > {DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeMilliseconds()}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var entries = new List<LeaderboardEntryHistory>();
			while (reader.Read())
			{
				var leh = new LeaderboardEntryHistory()
				{
					LeaderboardID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(2),
					FirstSeenTimeUTC = reader.GetUInt64(4),
					OldMilliseconds = reader.GetUInt64(5),
					NewMilliseconds = reader.GetUInt64(6),
					OldRank = reader.GetUInt32(7),
					NewRank = reader.GetUInt32(8),
					UpdatedTimeUTC = reader.GetUInt64(9),
				};
				leh.Leaderboard = new Leaderboard()
				{
					ID = leh.LeaderboardID,
					LevelName = reader.GetString(1),
				};
				leh.Player = new Player()
				{
					SteamID = leh.SteamID,
					Name = reader.GetString(3),
					SteamAvatar = reader.IsDBNull(10) ? null : reader.GetString(10),
				};
				entries.Add(leh);
			}
			reader.Close();
			Connection.Close();

			return entries;
		}

		public async Task<Dictionary<ulong, (long, long)>> GetPastWeeksImprovement(List<ulong> steamIDs = null, List<uint> leaderboardIDs = null)
		{
			var leaderboardClause = "";
			if (leaderboardIDs != null && leaderboardIDs.Count > 0)
			{
				leaderboardClause = $" AND l.ID IN ({string.Join(",", leaderboardIDs)})";
			}

			Connection.Open();
			var sql = @$"
				SELECT 
					leh.SteamID,
					SUM(IF(l.LevelType <> 2, OldMilliseconds, 0)) - SUM(IF(l.LevelType <> 2, NewMilliseconds, 0)) AS TimeImprovement,
					SUM(IF(l.LevelType = 2, NewMilliseconds, 0)) - SUM(IF(l.LevelType = 2, OldMilliseconds, 0)) AS StuntImprovement
				FROM LeaderboardEntryHistory leh 
				LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID 
				LEFT JOIN Players p on p.SteamID = leh.SteamID
				WHERE leh.UpdatedTimeUTC > {DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeMilliseconds()}{leaderboardClause}
				{(steamIDs == null ? "" : $"AND leh.SteamID IN ({string.Join(',', steamIDs)})")}
				GROUP BY leh.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalTimeImprovements = new Dictionary<ulong, (long, long)>();
			while (reader.Read())
			{
				globalTimeImprovements.Add(reader.GetUInt64(0), (reader.GetInt64(1), reader.GetInt64(2)));
			}
			reader.Close();
			Connection.Close();

			return globalTimeImprovements;
		}

		public async Task<List<LeaderboardEntryHistory>> GetWRLog(int limit = 20, List<uint> leaderboardIDs = null)
		{
			var leaderboardClause = "";
			if (leaderboardIDs != null && leaderboardIDs.Count > 0)
			{
				leaderboardClause = $"AND leh.LeaderboardID IN ({string.Join(",", leaderboardIDs)})";
			}
			else if (leaderboardIDs.Count == 0)
			{
				return new List<LeaderboardEntryHistory>();
			}

			Connection.Open();
			var sql = @$"SELECT 
				leh.LeaderboardID,
				l.LevelName,
				leh.SteamID,
				p.Name,
				leh.FirstSeenTimeUTC,
				OldMilliseconds,
				NewMilliseconds,
				OldRank,
				NewRank,
				UpdatedTimeUTC,
				p.SteamAvatar,
				l.ImageURL,
				l.LevelType
			FROM LeaderboardEntryHistory leh 
			LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID 
			LEFT JOIN Players p on p.SteamID = leh.SteamID 
			WHERE NewRank = 1 {leaderboardClause}
			ORDER BY leh.UpdatedTimeUTC DESC
			LIMIT {limit}";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var entries = new List<LeaderboardEntryHistory>();
			while (reader.Read())
			{
				var leh = new LeaderboardEntryHistory()
				{
					LeaderboardID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(2),
					FirstSeenTimeUTC = reader.GetUInt64(4),
					OldMilliseconds = reader.GetUInt64(5),
					NewMilliseconds = reader.GetUInt64(6),
					OldRank = reader.GetUInt32(7),
					NewRank = reader.GetUInt32(8),
					UpdatedTimeUTC = reader.GetUInt64(9),
				};
				leh.Leaderboard = new Leaderboard()
				{
					ID = leh.LeaderboardID,
					LevelName = reader.GetString(1),
					ImageURL = reader.GetString(11),
					LevelType = (LevelType) reader.GetUInt32(12),
				};
				leh.Player = new Player()
				{
					SteamID = leh.SteamID,
					Name = reader.GetString(3),
					SteamAvatar = reader.IsDBNull(10) ? null : reader.GetString(10),
				};
				entries.Add(leh);
			}
			reader.Close();
			Connection.Close();

			return entries;
		}
	}
}
