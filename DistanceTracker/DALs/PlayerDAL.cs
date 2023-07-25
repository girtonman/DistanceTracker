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
			var sql = $"SELECT ID, SteamID, Name, SteamAvatar FROM Players WHERE SteamID = {steamID}";
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

		public async Task<FunStats> GetFunStats(ulong steamID)
		{
			Connection.Open();
			var sql = $@"
				WITH
				  TracksCompleted AS(SELECT COUNT(*) AS TracksCompleted FROM LeaderboardEntries WHERE SteamID = {steamID}),
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
				SELECT* FROM TracksCompleted JOIN TotalImprovements JOIN FirstSeenTime JOIN MostActiveLevel";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			FunStats funStats = null;
			while (reader.Read())
			{
				funStats = new FunStats()
				{
					TracksCompleted = reader.GetInt32(0),
					TotalImprovements = reader.GetInt32(1),
					FirstSeenTimeUTC = reader.GetUInt64(2),
					MostImprovements = reader.GetInt32(3),
					MostImprovementsLevel = reader.GetString(4),
				};
			}
			reader.Close();
			Connection.Close();

			return funStats;
		}
	}
}
