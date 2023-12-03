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

		public async Task<List<LeaderboardEntryHistory>> GetRecentImprovements(int numRows = 20, ulong? steamID = null, uint? leaderboardID = null, uint? rankCutoff = null, ulong? after = null)
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
					p.SteamAvatar
				FROM LeaderboardEntryHistory leh
				LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID
				LEFT JOIN Players p on p.SteamID = leh.SteamID";

			// Add conditions
			var conditions = new List<string>();
			if (steamID.HasValue)
			{
				conditions.Add($"leh.SteamID = {steamID}");
			}
			if (leaderboardID.HasValue)
			{
				conditions.Add($"leh.LeaderboardID = {leaderboardID}");
			}
			if(rankCutoff.HasValue)
			{
				conditions.Add($"leh.NewRank <= {rankCutoff}");
			}
			if(after.HasValue)
			{
				conditions.Add($"leh.UpdatedTimeUTC > {after}");
			}
			for(var i = 0; i < conditions.Count; i++)
			{
				if(i == 0)
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

		public async Task<List<LeaderboardEntryHistory>> GetPastWeeksImprovements(ulong steamID)
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
				WHERE leh.SteamID = {steamID} 
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

		public async Task<Dictionary<ulong, long>> GetGlobalPastWeeksImprovement(List<ulong> steamIDs = null)
		{
			Connection.Open();
			var sql = @$"
				SELECT 
					leh.SteamID,
					SUM(OldMilliseconds) - SUM(NewMilliseconds)
				FROM LeaderboardEntryHistory leh 
				LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID 
				LEFT JOIN Players p on p.SteamID = leh.SteamID
				WHERE leh.UpdatedTimeUTC > {DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeMilliseconds()}
				{(steamIDs == null ? "" : $"AND leh.SteamID IN ({string.Join(',', steamIDs)})")}
				GROUP BY leh.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalTimeImprovements = new Dictionary<ulong, long>();
			while (reader.Read())
			{
				globalTimeImprovements.Add(reader.GetUInt64(0), reader.GetInt64(1));
			}
			reader.Close();
			Connection.Close();

			return globalTimeImprovements;
		}

		public async Task<List<LeaderboardEntryHistory>> GetWRLog(int limit = 20)
		{
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
				p.SteamAvatar
			FROM LeaderboardEntryHistory leh 
			LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID 
			LEFT JOIN Players p on p.SteamID = leh.SteamID 
			WHERE NewRank = 1
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
