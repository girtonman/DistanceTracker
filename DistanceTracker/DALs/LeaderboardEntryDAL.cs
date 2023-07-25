using DistanceTracker.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistanceTracker.DALs
{
	public class LeaderboardEntryDAL
	{
		private MySqlConnection Connection { get; set; }

		public LeaderboardEntryDAL(Settings settings)
		{
			Connection = new MySqlConnection(settings.ConnectionString);
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

		public async Task<List<GlobalRankedLeaderboardEntry>> GetGlobalLeaderboard(int numRows = 100)
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
						SUM(NoodlePoints) AS NoodlePoints,
						SUM(Milliseconds) AS Milliseconds,
						COUNT(*) AS NumTracksCompleted
					FROM(
						SELECT
							*,
							CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) AS `Rank` FROM LeaderboardEntries
						) ranks
					) le
					GROUP BY SteamID
					ORDER BY SUM(NoodlePoints) DESC
					LIMIT " + numRows + @"
				) global_leaderboard
				LEFT JOIN Players p ON p.SteamID = global_leaderboard.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalLeaderboardEntries = new List<GlobalRankedLeaderboardEntry>();
			while (reader.Read())
			{
				var grle = new GlobalRankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					TotalMilliseconds = reader.GetUInt64(3),
					NumTracksCompleted = reader.GetUInt32(4),
					PlayerRating = reader.GetDouble(5),
				};
				grle.Player = new Player()
				{
					SteamID = reader.GetUInt64(0),
					Name = reader.GetString(6),
					SteamAvatar = reader.IsDBNull(7) ? null : reader.GetString(7),
				};
				globalLeaderboardEntries.Add(grle);
			}
			reader.Close();
			Connection.Close();

			return globalLeaderboardEntries;
		}

		public async Task<ulong> GetOptimalTotalTime()
		{
			Connection.Open();
			var sql = @"
				SELECT
					SUM(le.MinMilliseconds)
				FROM Leaderboards l
				LEFT JOIN (
					SELECT
						LeaderboardID,
						MIN(Milliseconds) AS MinMilliseconds
					FROM LeaderboardEntries
					GROUP BY LeaderboardID
				) le ON le.LeaderboardID = l.ID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			while (reader.Read())
			{
				return reader.GetUInt64(0);
			}
			reader.Close();
			Connection.Close();

			return 0;
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
					fpc.Count
				FROM Players p
				JOIN (
					SELECT 
						SteamID, 
						COUNT(SteamID) Count
					FROM LeaderboardEntries le
					JOIN (
						SELECT 
							LeaderboardID,
							MIN(Milliseconds) MinTime
						FROM LeaderboardEntries
						GROUP BY LeaderboardID
					) lmt ON lmt.LeaderboardID = le.LeaderboardID AND lmt.MinTime = le.Milliseconds
					GROUP BY SteamID
				) fpc ON fpc.SteamID = p.SteamID
				ORDER BY fpc.Count DESC";

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

		public async Task<List<IGrouping<uint, HistogramDataPoint>>> GetHistogramDataPoints()
		{
			Connection.Open();
			var sql = $@"
				SELECT 
					FLOOR(Milliseconds/1000)*1000 AS BucketFloor,
					COUNT(*) AS BucketCount,
					LeaderboardID
				FROM (SELECT * FROM LeaderboardEntries WHERE Milliseconds <= 300000) clamped_le
				GROUP BY LeaderboardID, 1";

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var histogramData = new List<HistogramDataPoint>();
			while (reader.Read())
			{
				var dataPoint = new HistogramDataPoint()
				{
					BucketFloor = reader.GetUInt64(0),
					BucketCount = reader.GetUInt64(1),
					LeaderboardID = reader.GetUInt32(2),
				};

				histogramData.Add(dataPoint);
			}
			reader.Close();
			Connection.Close();

			return histogramData
				.OrderBy(x => x.BucketFloor)
				.GroupBy(x => x.LeaderboardID)
				.ToList();
		}

		public async Task<List<PercentileRank>> GetPercentileRanks(ulong steamID)
		{
			Connection.Open();
			var sql = @"
				SELECT
					le.Milliseconds,
					le.Percentile * 100 AS Percentile,
					le.LeaderboardID,
					l.LevelName,
					l.LeaderboardName,
					l.LevelSet
				FROM(
					SELECT
						*
					FROM(
							SELECT
								Milliseconds,
								LeaderboardID,
								SteamID,
								PERCENT_RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) AS Percentile
							FROM LeaderboardEntries
					) ranks
				) le
				LEFT JOIN Leaderboards l ON l.ID = le.LeaderboardID
				WHERE le.SteamID = " + steamID;

			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var percentileRanks = new List<PercentileRank>();
			while (reader.Read())
			{
				var percentileRank = new PercentileRank()
				{
					Milliseconds = reader.GetUInt64(0),
					Percentile = reader.GetFloat(1),
					LeaderboardID = reader.GetUInt32(2),
					LevelName = reader.GetString(3),
					LeaderboardName = reader.GetString(4),
					LevelSet = reader.GetString(5),
				};

				percentileRanks.Add(percentileRank);
			}
			reader.Close();
			Connection.Close();

			return percentileRanks;
		}

		public async Task<List<LeaderboardEntry>> GetOldestWRs(int numRows = 120)
		{
			Connection.Open();
			var sql = @$"
				SELECT le.LeaderboardID, l.LevelName, le.Milliseconds, le.SteamID, p.Name, le.FirstSeenTimeUTC, le.UpdatedTimeUTC, p.SteamAvatar
				FROM (SELECT LeaderboardID, MIN(Milliseconds) AS Milliseconds FROM LeaderboardEntries GROUP BY LeaderboardID) WR
				LEFT JOIN LeaderboardEntries le ON WR.LeaderboardID = le.LeaderboardID AND WR.Milliseconds = le.Milliseconds
				LEFT JOIN Leaderboards l on l.ID = le.LeaderboardID
				LEFT JOIN Players p on p.SteamID = le.SteamID
				ORDER BY le.UpdatedTimeUTC ASC";

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
	}
}
