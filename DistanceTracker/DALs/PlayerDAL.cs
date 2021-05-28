using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class PlayerDAL
	{
		private MySqlConnection Connection { get; set; }

		public PlayerDAL()
		{
			Connection = new MySqlConnection(Settings.ConnectionString);
		}

		public async Task<Player> GetPlayer(ulong steamID)
		{
			Connection.Open();
			var sql = $"SELECT ID, SteamID, Name FROM Players WHERE SteamID = {steamID}";
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
				};
			}
			reader.Close();
			Connection.Close();

			return player;
		}

		public async Task<List<Player>> SearchByName(string name)
		{
			Connection.Open();
			var sql = $"SELECT ID, SteamID, Name FROM Players WHERE `Name` LIKE '%{name}%'";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var players = new List<Player>();
			while (reader.Read())
			{
				players.Add(new Player()
				{
					ID = reader.GetUInt32(0),
					SteamID = reader.GetUInt64(1),
					Name = reader.GetString(2),
				});
			}
			reader.Close();
			Connection.Close();

			return players;
		}
	}
}
