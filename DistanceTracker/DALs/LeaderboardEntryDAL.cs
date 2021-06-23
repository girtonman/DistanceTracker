using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardEntryDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardEntryDAL()
		{
			Connection = new MySqlConnection(Settings.ConnectionString);
		}

		public async Task<Dictionary<ulong, LeaderboardEntry>> GetLeaderboardEntries(int leaderboardID)
		{
			Connection.Open();
			var sql = $"SELECT ID, LeaderboardID, Milliseconds, SteamID, FirstSeenTimeUTC, UpdatedTimeUTC FROM LeaderboardEntries WHERE LeaderboardID = {leaderboardID}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var leaderboardEntries = new Dictionary<ulong, LeaderboardEntry>();
			while (reader.Read())
			{
				var steamID = reader.GetUInt64(3);
				leaderboardEntries.Add(steamID, new LeaderboardEntry()
				{
					ID = reader.GetUInt32(0),
					LeaderboardID = reader.GetUInt32(1),
					Milliseconds = reader.GetUInt64(2),
					SteamID = steamID,
					FirstSeenTimeUTC = reader.GetUInt64(4),
					UpdatedTimeUTC = reader.GetUInt64(5),
				});
			}
			reader.Close();
			Connection.Close();

			return leaderboardEntries;
		}

		public async Task<List<LeaderboardEntry>> GetRecentFirstSightings(int numRows = 20, ulong? steamID = null, uint? leaderboardID = null)
		{
			Connection.Open();
			var sql = @$"
				SELECT le.LeaderboardID, l.LevelName, Milliseconds, le.SteamID, p.Name, le.FirstSeenTimeUTC, le.UpdatedTimeUTC, p.SteamAvatar FROM LeaderboardEntries le 
				LEFT JOIN Leaderboards l on l.ID = le.LeaderboardID 
				LEFT JOIN Players p on p.SteamID = le.SteamID ";

			if (steamID.HasValue)
			{
				sql += $"WHERE le.SteamID = {steamID} ";
			}

			if (leaderboardID.HasValue)
			{
				sql += $"WHERE le.LeaderboardID = {leaderboardID} ";
			}

			sql += $"ORDER BY le.ID DESC "
				+ $"LIMIT {numRows}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var leaderboardEntries = new List<LeaderboardEntry>();
			while (reader.Read())
			{
				var le = new LeaderboardEntry()
				{
					LeaderboardID = reader.GetUInt32(0),
					Milliseconds = reader.GetUInt64(2),
					SteamID = reader.GetUInt64(3),
					FirstSeenTimeUTC = reader.GetUInt64(5),
					UpdatedTimeUTC = reader.GetUInt64(6),
				};
				le.Leaderboard = new Leaderboard()
				{
					ID = le.LeaderboardID,
					LevelName = reader.GetString(1),
				};
				le.Player = new Player()
				{
					SteamID = le.SteamID,
					Name = reader.GetString(4),
					SteamAvatar = reader.IsDBNull(7) ? null : reader.GetString(7),
				};
				leaderboardEntries.Add(le);
			}
			reader.Close();
			Connection.Close();

			return leaderboardEntries;
		}

		public async Task<List<RankedLeaderboardEntry>> GetGlobalLeaderboard(int numRows = 100)
		{
			Connection.Open();
			var sql = @"
				SELECT global_leaderboard.*,
					ROUND(NoodlePoints / 1200.0, 2) as PlayerRating,
					p.Name,
					p.SteamAvatar
				FROM(
					SELECT
						SteamID,
						RANK() OVER(
						  ORDER BY SUM(NoodlePoints) DESC
						) as GlobalRank,
						SUM(NoodlePoints) as NoodlePoints
					FROM(
						SELECT
							*,
							CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) as `Rank` FROM LeaderboardEntries
						) ranks
					) le
					GROUP BY SteamID
					ORDER BY SUM(NoodlePoints) DESC
					LIMIT " + numRows + @"
				) global_leaderboard
				LEFT JOIN Players p ON p.SteamID = global_leaderboard.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalLeaderboardEntries = new List<RankedLeaderboardEntry>();
			while (reader.Read())
			{
				var gle = new RankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					PlayerRating = reader.GetDouble(3),
				};
				gle.Player = new Player()
				{
					SteamID = reader.GetUInt64(0),
					Name = reader.GetString(4),
					SteamAvatar = reader.IsDBNull(5) ? null : reader.GetString(5),
				};
				globalLeaderboardEntries.Add(gle);
			}
			reader.Close();
			Connection.Close();

			return globalLeaderboardEntries;
		}

		public async Task<RankedLeaderboardEntry> GetGlobalRankingForPlayer(ulong steamID)
		{
			Connection.Open();
			var sql = @"
				SELECT global_leaderboard.*,
					ROUND(NoodlePoints / 1200.0, 2) as PlayerRating
				FROM(
					SELECT
						SteamID,
						RANK() OVER(
						  ORDER BY SUM(NoodlePoints) DESC
						) as GlobalRank,
						SUM(NoodlePoints) as NoodlePoints
					FROM(
						SELECT
							*,
							CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) as `Rank` FROM LeaderboardEntries
						) ranks
					) le
					GROUP BY SteamID
				) global_leaderboard WHERE SteamID = " + steamID;

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalRanking = new RankedLeaderboardEntry();
			while (reader.Read())
			{
				globalRanking = new RankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					PlayerRating = reader.GetDouble(3),
				};
			}
			reader.Close();
			Connection.Close();

			return globalRanking;
		}

		public async Task<RankedLeaderboardEntry> GetGlobalRankingForPoints(int points)
		{
			Connection.Open();
			var sql = @"
				SELECT global_leaderboard.*,
					ROUND(NoodlePoints / 1200.0, 2) as PlayerRating
				FROM(
					SELECT
						SteamID,
						RANK() OVER(
						  ORDER BY SUM(NoodlePoints) DESC
						) as GlobalRank,
						SUM(NoodlePoints) as NoodlePoints
					FROM(
						SELECT
							*,
							CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) as `Rank` FROM LeaderboardEntries
						) ranks
					) le
					GROUP BY SteamID
				) global_leaderboard
				WHERE NoodlePoints <= " + points + @"
				ORDER BY NoodlePoints DESC
				LIMIT 1";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalRanking = new RankedLeaderboardEntry();
			while (reader.Read())
			{
				globalRanking = new RankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					PlayerRating = reader.GetDouble(3),
				};
			}
			reader.Close();
			Connection.Close();

			return globalRanking;
		}

		public async Task<List<RankedLeaderboardEntry>> GetRankedLeaderboardEntriesForPlayer(ulong steamID)
		{
			Connection.Open();
			var sql = @"
				SELECT
					le.`Rank`,
					le.NoodlePoints,
					ROUND(NoodlePoints / 10.0, 2) as PlayerRating,
					le.LeaderboardID,
					le.SteamID,
					l.LevelName,
					p.Name,
					p.SteamAvatar
				FROM(
					SELECT
						*,
						CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
					FROM(
							SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) as `Rank` FROM LeaderboardEntries
					) ranks
				) le
				LEFT JOIN Leaderboards l ON l.ID = le.LeaderboardID
				LEFT JOIN Players p ON p.SteamID = le.SteamID
				WHERE le.SteamID = " + steamID;

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var rankedEntries = new List<RankedLeaderboardEntry>();
			while (reader.Read())
			{
				var rle = new RankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(0),
					NoodlePoints = reader.GetDouble(1),
					PlayerRating = reader.GetDouble(2),
				};
				rle.Leaderboard = new Leaderboard()
				{
					ID = reader.GetUInt32(3),
					LevelName = reader.GetString(5),
				};
				rle.Player = new Player()
				{
					SteamID = reader.GetUInt64(4),
					Name = reader.GetString(6),
					SteamAvatar = reader.IsDBNull(7) ? null : reader.GetString(7),
				};

				rankedEntries.Add(rle);
			}
			reader.Close();
			Connection.Close();

			return rankedEntries;
		}

		public async Task<List<RankedLeaderboardEntry>> GetRankedLeaderboardEntriesForLevel(uint leaderboardID)
		{
			Connection.Open();
			var sql = $@"
				SELECT
					le.`Rank`,
					le.NoodlePoints,
					ROUND(NoodlePoints / 10.0, 2) as PlayerRating,
					le.SteamID,
					p.Name,
					le.Milliseconds,
					le.UpdatedTimeUTC,
					p.SteamAvatar
				FROM(
					SELECT
						*,
						CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
					FROM(
							SELECT Milliseconds, SteamID, RANK() OVER(ORDER BY Milliseconds ASC) as `Rank`, FirstSeenTimeUTC, UpdatedTimeUTC FROM LeaderboardEntries WHERE LeaderboardID = {leaderboardID}
					) ranks
				) le
				LEFT JOIN Players p ON p.SteamID = le.SteamID
				LIMIT 100";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var rankedEntries = new List<RankedLeaderboardEntry>();
			while (reader.Read())
			{
				var rle = new RankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(0),
					NoodlePoints = reader.GetDouble(1),
					PlayerRating = reader.GetDouble(2),
					Milliseconds = reader.GetUInt64(5),
					UpdatedTimeUTC = reader.GetUInt64(6),
				};
				rle.Player = new Player()
				{
					SteamID = reader.GetUInt64(3),
					Name = reader.GetString(4),
					SteamAvatar = reader.IsDBNull(7) ? null : reader.GetString(7),
				};

				rankedEntries.Add(rle);
			}
			reader.Close();
			Connection.Close();

			return rankedEntries;
		}

		public async Task<List<WinnersCircleEntry>> GetGlobalWinnersCircle()
		{
			Connection.Open();
			var sql = $@"
				SELECT
					p.Name,
					COUNT(*) AS `Count`
				FROM
				(
					SELECT 
						l.ID AS LeaderboardID,
						(
		 					SELECT SteamID
		 					FROM LeaderboardEntries
		 					WHERE LeaderboardID = l.ID
		 					ORDER BY Milliseconds ASC
		 					LIMIT 1
						) AS SteamID
					FROM Leaderboards l
				) fpe
				LEFT JOIN Players p ON p.SteamID = fpe.SteamID
				WHERE p.Name IS NOT NULL
				GROUP BY p.Name
				ORDER BY `Count` DESC";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var winnersCircle = new List<WinnersCircleEntry>();
			while (reader.Read())
			{
				var entry = new WinnersCircleEntry()
				{
					Name = reader.GetString(0),
					Count = reader.GetInt32(1),
				};

				winnersCircle.Add(entry);
			}
			reader.Close();
			Connection.Close();

			return winnersCircle;
		}
	}
}
