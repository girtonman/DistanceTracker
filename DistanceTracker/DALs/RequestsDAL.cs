using DistanceTracker.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class RequestsDAL
	{
		private MySqlConnection Connection { get; set; }

		public RequestsDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<RequestedUpdate> GetRequest(uint requestID)
		{
			Connection.Open();

			var sql = $"SELECT ID, LeaderboardID, RequestedTimeUTC, ProcessedTimeUTC FROM RequestedUpdates WHERE ID = {requestID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			RequestedUpdate request = null;
			while (reader.Read())
			{
				request = new RequestedUpdate()
				{
					ID = reader.GetUInt32(0),
					LeaderboardID = reader.GetUInt32(1),
					RequestedTimeUTC = reader.GetUInt64(2),
					ProcessedTimeUTC = reader.IsDBNull(3) ? (ulong?)null : reader.GetUInt64(3),
				};
			}

			Connection.Close();
			return request;
		}

		public async Task<uint?> GetOpenRequestForLeaderboardID(uint leaderboardID)
		{
			Connection.Open();

			var sql = $"SELECT ID FROM RequestedUpdates WHERE LeaderboardID = {leaderboardID} AND ProcessedTimeUTC IS NULL";
			var command = new MySqlCommand(sql, Connection);
			var result = await command.ExecuteScalarAsync();

			Connection.Close();
			return (uint?)result;
		}

		public async Task<uint> AddRequest(uint leaderboardID)
		{
			var existingRequest = await GetOpenRequestForLeaderboardID(leaderboardID);
			if (existingRequest != null)
			{
				return (uint)existingRequest;
			}

			Connection.Open();

			var sql = $"INSERT INTO RequestedUpdates(LeaderboardID, RequestedTimeUTC) VALUES ({leaderboardID}, {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()})";
			var command = new MySqlCommand(sql, Connection);
			await command.ExecuteNonQueryAsync();
			
			Connection.Close();
			
			return (uint)command.LastInsertedId;
		}
	}
}
