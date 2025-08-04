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

		public async Task<List<LeaderboardEntry>> GetRecentFirstSightings(int numRows = 20, ulong? steamID = null, List<uint> leaderboardIDs = null, ulong? after = null)
		{
			Connection.Open();
			var sql = @$"
				SELECT le.LeaderboardID, l.LevelName, Milliseconds, le.SteamID, p.Name, le.FirstSeenTimeUTC, le.UpdatedTimeUTC, p.SteamAvatar, l.ImageURL, l.LevelType FROM LeaderboardEntries le 
				LEFT JOIN Leaderboards l on l.ID = le.LeaderboardID 
				LEFT JOIN Players p on p.SteamID = le.SteamID ";

			var conditions = new List<string>();
			if (steamID.HasValue)
			{
				conditions.Add($"le.SteamID = {steamID}");
			}

			if (leaderboardIDs != null && leaderboardIDs.Count > 0)
			{
				conditions.Add($"le.LeaderboardID IN ({string.Join(",", leaderboardIDs)})");
			}

			if (after.HasValue)
			{
				conditions.Add($"le.FirstSeenTimeUTC > {after}");
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

			sql += $" ORDER BY le.ID DESC LIMIT {numRows}";
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
					ImageURL = reader.GetString(8),
					LevelType = (LevelType)reader.GetUInt32(9),
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

		public async Task<List<OverviewRankedLeaderboardEntry>> GetGlobalLeaderboard(List<uint> leaderboardIDs, int numRows = 100)
		{
			if(leaderboardIDs.Count == 0)
			{
				return new List<OverviewRankedLeaderboardEntry>();
			}

			Connection.Open();
			var sql = @$"
				SELECT global_leaderboard.*,
					ROUND(NoodlePoints / (10.0 * {leaderboardIDs.Count}), 2) as PlayerRating,
					p.Name,
					p.SteamAvatar
				FROM(
					SELECT
						SteamID,
						RANK() OVER(
						  ORDER BY SUM(NoodlePoints) DESC
						) as GlobalRank,
						SUM(NoodlePoints) AS NoodlePoints,
						SUM(IF(l.LevelType <> 2, Milliseconds, 0)) AS Milliseconds,
						SUM(IF(l.LevelType = 2, Milliseconds, 0)) AS StuntScore,
						COUNT(*) AS NumTracksCompleted
					FROM(
						SELECT
							*,
							CASE WHEN `Rank` is NULL OR `Rank` > 1000 THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) AS `Rank` FROM LeaderboardEntries
								WHERE LeaderboardID IN ({string.Join(',', leaderboardIDs)})
						) ranks
					) le
					LEFT JOIN Leaderboards l ON l.ID = le.LeaderboardID
					GROUP BY SteamID
					ORDER BY SUM(NoodlePoints) DESC
					LIMIT {numRows}
				) global_leaderboard
				LEFT JOIN Players p ON p.SteamID = global_leaderboard.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalLeaderboardEntries = new List<OverviewRankedLeaderboardEntry>();
			while (reader.Read())
			{
				var grle = new OverviewRankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					TotalMilliseconds = reader.GetUInt64(3),
					TotalStuntScore = reader.GetUInt64(4),
					NumTracksCompleted = reader.GetUInt32(5),
					PlayerRating = reader.GetDouble(6),
				};
				grle.Player = new Player()
				{
					SteamID = reader.GetUInt64(0),
					Name = reader.GetString(7),
					SteamAvatar = reader.IsDBNull(8) ? null : reader.GetString(8),
				};
				globalLeaderboardEntries.Add(grle);
			}
			reader.Close();
			Connection.Close();

			return globalLeaderboardEntries;
		}

		public async Task<List<OverviewRankedLeaderboardEntry>> GetMultiLevelLeaderboard(uint rankCutoff, List<uint> leaderboardIDs, int numRows = 100)
		{
			if (leaderboardIDs == null || leaderboardIDs.Count == 0)
			{
				return new List<OverviewRankedLeaderboardEntry>();
			}

			Connection.Open();
			var sql = @$"
				SELECT global_leaderboard.*,
					ROUND(NoodlePoints / {leaderboardIDs.Count() * 10}.0, 2) as PlayerRating,
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
							CASE WHEN `Rank` is NULL OR `Rank` > {rankCutoff} THEN 0 ELSE ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / {rankCutoff}) - 1.0), 2)))) END AS NoodlePoints
						FROM(
								SELECT Milliseconds, LeaderboardID, SteamID, RANK() OVER(PARTITION BY LeaderboardID ORDER BY Milliseconds ASC) AS `Rank` FROM LeaderboardEntries
								WHERE LeaderboardID IN ({string.Join(",", leaderboardIDs)})
						) ranks
					) le
					GROUP BY SteamID
					ORDER BY SUM(NoodlePoints) DESC
					LIMIT " + numRows + @"
				) global_leaderboard
				LEFT JOIN Players p ON p.SteamID = global_leaderboard.SteamID";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var overviewLeaderboardEntries = new List<OverviewRankedLeaderboardEntry>();
			while (reader.Read())
			{
				var orle = new OverviewRankedLeaderboardEntry()
				{
					Rank = reader.GetInt32(1),
					NoodlePoints = reader.GetDouble(2),
					TotalMilliseconds = reader.GetUInt64(3),
					NumTracksCompleted = reader.GetUInt32(4),
					PlayerRating = reader.GetDouble(5),
				};
				orle.Player = new Player()
				{
					SteamID = reader.GetUInt64(0),
					Name = reader.GetString(6),
					SteamAvatar = reader.IsDBNull(7) ? null : reader.GetString(7),
				};
				overviewLeaderboardEntries.Add(orle);
			}
			reader.Close();
			Connection.Close();

			return overviewLeaderboardEntries;
		}

		public async Task<uint> GetMaxEntryCount(List<uint> leaderboardIDs)
		{
			var whereClause = "";
			if (leaderboardIDs != null && leaderboardIDs.Count > 0)
			{
				whereClause = $"WHERE LeaderboardID IN ({string.Join(",", leaderboardIDs)})";
			}

			Connection.Open();
			var sql = $"SELECT COUNT(*) FROM LeaderboardEntries {whereClause} GROUP BY LeaderboardID ORDER BY 1 DESC LIMIT 1";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			uint maxEntries = 0;
			while (reader.Read())
			{
				maxEntries = reader.GetUInt32(0);
			}
			reader.Close();
			Connection.Close();

			return maxEntries > 0 ? maxEntries : 1; // return 1 for safety from divide by zeroes later
		}

		public async Task<ulong> GetOptimalTotal(List<uint> leaderboardIDs, bool isStunt = false)
		{
			string innerWhereClause;
			if (leaderboardIDs.Count > 0)
			{
				innerWhereClause = $"WHERE LeaderboardID IN ({string.Join(",", leaderboardIDs)})";
			}
			else
			{
				// no point in querying if there are no leaderboards specified
				return 0;
			}

			// Filter based on stunt flag
			string outerWhereClause = isStunt ? "WHERE l.LevelType = 2" : "WHERE l.LevelType <> 2";

			Connection.Open();
			var sql = @$"
				SELECT
					SUM(le.OptimalMilliseconds)
				FROM Leaderboards l
				LEFT JOIN (
					SELECT
						LeaderboardID,
						{(isStunt ? "MAX(Milliseconds) AS OptimalMilliseconds" : "MIN(Milliseconds) AS OptimalMilliseconds")}
					FROM LeaderboardEntries
					{innerWhereClause}
					GROUP BY LeaderboardID
				) le ON le.LeaderboardID = l.ID
				{outerWhereClause}";
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			ulong optimalTotal = 0;
			while (reader.Read())
			{
				optimalTotal = reader.IsDBNull(0) ? 0 : reader.GetUInt64(0);
			}
			reader.Close();
			Connection.Close();

			return optimalTotal;
		}

		public async Task<RankedLeaderboardEntry> GetGlobalRankingForPlayer(ulong steamID, List<Leaderboard> leaderboards)
		{
			// Hide our optimization sins in a nice little package
			// This creates SQL that will emulate the functionality of a materialized view (since mysql doesn't have those)
			// so that we can force the RANK() to be limited to the least amount of rows possible
			// This cursed optimization brought to you by noodle_beard and JnvSor
			var materializedUnion = string.Join(" UNION ALL ", leaderboards
				.Select(x => $"(SELECT Milliseconds, LeaderboardID, SteamID, ROW_NUMBER() AS `Rank` FROM LeaderboardEntries WHERE LeaderboardID = {x.ID} ORDER BY Milliseconds {(x.LevelType == LevelType.Stunt ? "DESC" : "ASC")} LIMIT 1000)"));

			var sql = $@"
				WITH limited_entries AS
				(
					{materializedUnion}
				),
				le AS
				(
					SELECT
						*,
						ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) AS NoodlePoints
					FROM limited_entries
				),
				global_leaderboard AS
				(
					SELECT
						SteamID,
						SUM(NoodlePoints) as TotalNoodlePoints,
						RANK() OVER(ORDER BY SUM(NoodlePoints) DESC) AS GlobalRank
					FROM le
					GROUP BY SteamID
				)

				SELECT 
					*,
					ROUND(TotalNoodlePoints / 1200.0, 2) as PlayerRating
				FROM global_leaderboard
				WHERE SteamID = {steamID}";

			Connection.Open();
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalRanking = new RankedLeaderboardEntry();
			while (reader.Read())
			{
				globalRanking = new RankedLeaderboardEntry()
				{
					NoodlePoints = reader.GetDouble(1),
					Rank = reader.GetInt32(2),
					PlayerRating = reader.GetDouble(3),
				};
			}
			reader.Close();
			Connection.Close();

			return globalRanking;
		}

		public async Task<RankedLeaderboardEntry> GetGlobalRankingForPoints(int points, List<Leaderboard> leaderboards)
		{		
			// Hide our optimization sins in a nice little package
			// This creates SQL that will emulate the functionality of a materialized view (since mysql doesn't have those)
			// so that we can force the RANK() to be limited to the least amount of rows possible
			// This cursed optimization brought to you by noodle_beard and JnvSor
			var materializedUnion = string.Join(" UNION ALL ", leaderboards
				.Select(x => $"(SELECT Milliseconds, LeaderboardID, SteamID, ROW_NUMBER() AS `Rank` FROM LeaderboardEntries WHERE LeaderboardID = {x.ID} ORDER BY Milliseconds {(x.LevelType == LevelType.Stunt ? "DESC" : "ASC")} LIMIT 1000)"));

			var sql = $@"
				WITH limited_entries AS
				(
					{materializedUnion}
				),
				le AS
				(
					SELECT
						*,
						ROUND(1000.0 * (1.0 - SQRT(1.0 - POW((((`Rank` -1.0) / 1000.0) - 1.0), 2)))) AS NoodlePoints
					FROM limited_entries
				),
				global_leaderboard AS
				(
					SELECT
						SteamID,
						SUM(NoodlePoints) as TotalNoodlePoints,
						RANK() OVER(ORDER BY SUM(NoodlePoints) DESC) AS GlobalRank
					FROM le
					GROUP BY SteamID
				)

				SELECT
					*,
					ROUND(TotalNoodlePoints / 1200.0, 2) as PlayerRating
				FROM global_leaderboard
				WHERE TotalNoodlePoints <= {points}
				ORDER BY TotalNoodlePoints DESC
				LIMIT 1";

			Connection.Open();
			var command = new MySqlCommand(sql, Connection);
			var reader = await command.ExecuteReaderAsync();

			var globalRanking = new RankedLeaderboardEntry();
			while (reader.Read())
			{
				globalRanking = new RankedLeaderboardEntry()
				{
					NoodlePoints = reader.GetDouble(1),
					Rank = reader.GetInt32(2),
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
					p.SteamAvatar,
					l.ImageURL
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
					ImageURL = reader.GetString(8),
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

		public async Task<List<RankedLeaderboardEntry>> GetRankedLeaderboardEntriesForLevel(uint leaderboardID, bool reverse = false)
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
							SELECT Milliseconds, SteamID, RANK() OVER(ORDER BY Milliseconds {(reverse ? "DESC" : "ASC")}) as `Rank`, FirstSeenTimeUTC, UpdatedTimeUTC FROM LeaderboardEntries WHERE LeaderboardID = {leaderboardID}
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

		public async Task<List<WinnersCircleEntry>> GetGlobalWinnersCircle(List<uint> leaderboardIDs)
		{
			if(leaderboardIDs.Count == 0)
			{
				return new List<WinnersCircleEntry>();
			}

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
						WHERE LeaderboardID IN ({string.Join(',', leaderboardIDs)})
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

		public async Task<List<WinnersCircleEntry>> GetMultiLevelWinnersCircle(List<uint> leaderboardIDs)
		{
			if (leaderboardIDs == null || leaderboardIDs.Count == 0)
			{
				return new List<WinnersCircleEntry>();
			}

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
						WHERE LeaderboardID IN (" + string.Join(",", leaderboardIDs) + @")
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
					l.LevelSet,
					l.ImageURL
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
					LevelSet = reader.IsDBNull(5) ? "None" : reader.GetString(5),
					ImageURL = reader.IsDBNull(6) ? null : reader.GetString(6),
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
				SELECT le.LeaderboardID, l.LevelName, le.Milliseconds, le.SteamID, p.Name, le.FirstSeenTimeUTC, le.UpdatedTimeUTC, p.SteamAvatar, l.ImageURL, l.LevelType
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
					ImageURL = reader.GetString(8),
					LevelType = (LevelType) reader.GetUInt32(9),
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
