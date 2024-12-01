using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class PlayerDAL
	{
		private MySqlConnection Connection { get; set; }

		public PlayerDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<Player> GetPlayer(ulong steamID)
		{
			Connection.Open();
			var sql = $"SELECT ID, SteamID, Name, SteamAvatar, SteamBackground FROM Players WHERE SteamID = {steamID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			Player player = null;
			while (reader.Read())
			{
				player = new Player()
				{
					ID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(1),
					Name = reader.GetString(2),
					SteamAvatar = reader.IsDBNull(3) ? null : reader.GetString(3),
					SteamBackground = reader.IsDBNull(4) ? null : reader.GetString(4),
				};
			}
			reader.Close();
			Connection.Close();

			return player;
		}

		public async Task<List<Player>> SearchByName(string name)
		{
			Connection.Open();
			var sql = $"SELECT ID, SteamID, Name, SteamAvatar FROM Players WHERE `Name` LIKE @search LIMIT 50";
			var command = new MySqlCommand(sql, Connection);
			command.Parameters.AddWithValue("@search", $"%{name}%");
			var reader = await command.ExecuteReaderAsync();

			var players = new List<Player>();
			while (reader.Read())
			{
				players.Add(new Player()
				{
					ID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(1),
					Name = reader.GetString(2),
					SteamAvatar = reader.IsDBNull(3) ? null : reader.GetString(3),
				});
			}
			reader.Close();
			Connection.Close();

			return players;
		}

		public async Task UpdateSteamAvatar(ulong steamID, string steamAvatar)
		{
			Connection.Open();

			var sql = $"UPDATE Players SET SteamAvatar = @steamAvatar WHERE SteamID = {steamID}";

			var command = new MySqlCommand(sql, Connection);
			command.Parameters.AddWithValue("@steamAvatar", steamAvatar);
			await command.ExecuteNonQueryAsync();

			Connection.Close();
		}

		public async Task UpdateSteamName(ulong steamID, string steamName)
		{
			Connection.Open();

			var sql = $"UPDATE Players SET Name = '@steamName' WHERE SteamID = {steamID}";

			var command = new MySqlCommand(sql, Connection);
			command.Parameters.AddWithValue("@steamName", steamName);
			await command.ExecuteNonQueryAsync();

			Connection.Close();
		}

		public async Task UpdateSteamBackground(ulong steamID, string steamBackground)
		{
			Connection.Open();

			var sql = $"UPDATE Players SET SteamBackground = @steamBackground WHERE SteamID = {steamID}";

			var command = new MySqlCommand(sql, Connection);
			command.Parameters.AddWithValue("@steamBackground", steamBackground);
			await command.ExecuteNonQueryAsync();

			Connection.Close();
		}

		public async Task<FunStats> GetFunStats(ulong steamID)
		{
			Connection.Open();
			var sql = $@"
				WITH
				  OfficialTracksCompleted AS(SELECT COUNT(*) AS OfficialTracksCompleted FROM LeaderboardEntries le LEFT JOIN Leaderboards l ON le.LeaderboardID = l.ID WHERE l.IsOfficial = 1 AND SteamID = {steamID}),
				  UnofficialTracksCompleted AS(SELECT COUNT(*) AS UnofficialTracksCompleted FROM LeaderboardEntries le LEFT JOIN Leaderboards l ON le.LeaderboardID = l.ID WHERE l.IsOfficial = 0 AND SteamID = {steamID}),
				  TotalImprovements AS(SELECT COUNT(*) AS TotalImprovements FROM LeaderboardEntryHistory WHERE SteamID = {steamID}),
				  FirstSeenTime AS(SELECT FirstSeenTimeUTC FROM LeaderboardEntries WHERE SteamID = {steamID} ORDER BY FirstSeenTimeUTC ASC LIMIT 1),
				  MostActiveLevel AS(
				  SELECT
						COUNT(*) AS MostImprovements,
						LevelName AS MostImprovementsLevel
					FROM (
						SELECT
							l.LevelName
						FROM LeaderboardEntryHistory leh
						LEFT JOIN Leaderboards l ON l.ID = leh.LeaderboardID
						WHERE SteamID = {steamID}
					) sub
					GROUP BY LevelName
					LIMIT 1
				)
				SELECT* FROM OfficialTracksCompleted JOIN UnofficialTracksCompleted JOIN TotalImprovements JOIN FirstSeenTime JOIN MostActiveLevel";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			FunStats funStats = null;
			while (reader.Read())
			{
				funStats = new FunStats()
				{
					OfficialTracksCompleted = reader.GetInt32(0),
					UnofficialTracksCompleted = reader.GetInt32(1),
					TotalImprovements = reader.GetInt32(2),
					FirstSeenTimeUTC = reader.GetUInt64(3),
					MostImprovements = reader.GetInt32(4),
					MostImprovementsLevel = reader.GetString(5),
				};
			}
			reader.Close();
			Connection.Close();

			return funStats;
		}
	}
}
