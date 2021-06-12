using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardEntryHistoryDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardEntryHistoryDAL()
		{
			Connection = new MySqlConnection(Settings.ConnectionString);
		}

		public async Task<List<LeaderboardEntryHistory>> GetRecentImprovements(int numRows = 20, ulong? steamID = null, uint? leaderboardID = null)
		{
			Connection.Open();
			var sql = "SELECT leh.LeaderboardID, l.LevelName, leh.SteamID, p.Name, leh.FirstSeenTimeUTC, OldMilliseconds, NewMilliseconds, OldRank, NewRank, UpdatedTimeUTC, p.SteamAvatar FROM LeaderboardEntryHistory leh "
				+ "LEFT JOIN Leaderboards l on l.ID = leh.LeaderboardID "
				+ "LEFT JOIN Players p on p.SteamID = leh.SteamID ";

			if (steamID.HasValue)
			{
				sql += $"WHERE leh.SteamID = {steamID} ";
			}

			if (leaderboardID.HasValue)
			{
				sql += $"WHERE leh.LeaderboardID = {leaderboardID} ";
			}

			sql += $"ORDER BY leh.ID DESC LIMIT {numRows}";
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
	}
}
