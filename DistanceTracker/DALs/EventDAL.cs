using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class EventDAL
	{
		private MySqlConnection Connection { get; set; }

		public EventDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
		}

		public async Task<Event> GetEventDetails(uint eventID)
		{
			var eventDetails = new Event();
			Connection.Open();

			var sql = $"SELECT ID, Name, StartTimeUTC, EndTimeUTC, EventBackgroundImageURL FROM Events WHERE ID = {eventID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			while (reader.Read())
			{
				eventDetails.ID = reader.GetUInt32(0);
				eventDetails.Name = reader.GetString(1);
				eventDetails.StartTimeUTC = reader.IsDBNull(2) ? (ulong?)null : reader.GetUInt64(2);
				eventDetails.EndTimeUTC = reader.IsDBNull(3) ? (ulong?)null : reader.GetUInt64(3);
				eventDetails.EventBackgroundImageURL = reader.GetString(4);
			}

			Connection.Close();
			return eventDetails;
		}

		public async Task<List<uint>> GetEventLeaderboards(uint eventID)
		{
			var leaderboardIDs = new List<uint>();
			Connection.Open();

			var sql = $"SELECT LeaderboardID FROM EventLeaderboards WHERE EventID = {eventID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			while (reader.Read())
			{
				leaderboardIDs.Add(reader.GetUInt32(0));
			}

			Connection.Close();
			return leaderboardIDs;
		}
	}
}
