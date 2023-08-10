using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Dto;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query;

public static class QueryUserStats {
    public static async Task<Result<long, string>> GetUserRank(NpgsqlConnection db, long userId) {
        var sql = @$"
SELECT UserRank as Value
FROM (
    SELECT UserId, rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as UserRank
    FROM UserStats
    JOIN UserInfo bu on UserStats.UserId = bu.UserId
    WHERE bu.banned = false) as t
WHERE UserId = {userId};
";
        return (await db.SafeQueryFirstAsync<BoxLong>(sql)).Map(x => x.Value);
    }

    public static async Task<ResultErr<string>> InsertAsync(NpgsqlConnection db, UserStats userStats) {
        return await db.SafeInsertAsync(userStats);
    }

    public static async Task<ResultErr<string>> UpdateStatsFromScoreAsync(
        NpgsqlConnection db,
        long userId,
        PlayScoreDto now,
        PlayScoreDto? old = null) {
        var dif = old is null ? now : now - old;
        var playcount = old is null ? 1 : 0;

        return await db.SafeQueryAsync(@$"
UPDATE UserStats 
SET
    OverallPlaycount = OverallPlaycount + {playcount}, 
    OverallScore = OverallScore + {dif.Score}, 
    OverallAccuracy = OverallAccuracy + {dif.Accuracy}, 
    OverallCombo = OverallCombo + {dif.Combo}, 
    OverallXss = OverallXss + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.XSS)}, 
    OverallSs = OverallSs + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.SS)}, 
    OverallXs = OverallXs + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.XS)}, 
    OverallS = OverallS + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.S)}, 
    OverallA = OverallA + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.A)}, 
    OverallB = OverallB + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.B)}, 
    OverallC = OverallC + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.C)}, 
    OverallD = OverallD + {dif.EqAsInt(PlayScoreDto.EPlayScoreMark.D)}, 
    OverallHits = OverallHits + {dif.GetValue(PlayScoreDto.EPlayScore.Hits)},  
    Overall300 = Overall300 + {dif.GetValue(PlayScoreDto.EPlayScore.N300)}, 
    Overall100 = Overall100 + {dif.GetValue(PlayScoreDto.EPlayScore.N100)}, 
    Overall50 = Overall50 + {dif.GetValue(PlayScoreDto.EPlayScore.N50)}, 
    OverallGeki = OverallGeki + {dif.Geki}, 
    OverallKatu = OverallKatu + {dif.Katu}, 
    OverallMiss = OverallMiss + {dif.Miss}
WHERE USERID = {userId}
"
        );
    }


    public static async Task<Result<Option<UserStats>, string>> GetBblUserStatsByUserIdAsync(
        NpgsqlConnection db,
        long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<UserStats>(@$"
SELECT * FROM UserStats
         WHERE UserId = {userId}
         LIMIT 1
"
        );
    }

    public static async Task<Result<IEnumerable<LeaderBoardUser>, string>> LeaderBoardFilterCountry(
        NpgsqlConnection db,
        int limit,
        string countryNameShort) {
        var sql = @$"
SELECT rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber, 
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       OverallAccuracy
FROM UserStats
         JOIN UserInfo bu on bu.UserId = UserStats.UserId
WHERE Banned = false AND bu.Region = 'DE'
ORDER BY RankNumber ASC
LIMIT {limit};
";
        return await db.SafeQueryAsync<LeaderBoardUser>(
            sql,
            new { Region = countryNameShort }
        );
    }

    public static async Task<Result<Option<LeaderBoardUser>, string>> LeaderBoardUserRank(
        NpgsqlConnection db,
        long userId) {
        var sql = @"
SELECT UserId,
       RankNumber,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       bu.OverallAccuracy
FROM (SELECT rank()
             OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber,
             bu.UserId, Username, bu.Region
      FROM UserStats
               JOIN UserInfo bu on bu.UserId = UserStats.UserId
      WHERE Banned = false
      ORDER BY RankNumber ASC) xx FULL JOIN UserStats bu on bu.UserId = xx.UserId
WHERE xx.UserId = @0
";
        return await db.SafeQueryFirstOrDefaultAsync<LeaderBoardUser>(sql, new { UserId = userId });
    }

    public static async Task<Result<IEnumerable<LeaderBoardUser>, string>> LeaderBoardNoFilter(
        NpgsqlConnection db,
        int limit) {
        var sql = @$"
SELECT rank() OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber, 
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       OverallAccuracy
FROM UserStats
         JOIN UserInfo bu on bu.UserId = UserStats.UserId
WHERE banned = false
ORDER BY RankNumber ASC
LIMIT {limit};
";
        return await db.SafeQueryAsync<LeaderBoardUser>(sql);
    }

    public static async Task<Result<IEnumerable<LeaderBoardUser>, string>> LeaderBoardSearchUser(
        NpgsqlConnection db,
        long limit,
        string query) {
        var sql = @$"
SELECT RankNumber,
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       bu.OverallAccuracy
FROM (SELECT rank()
             OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber,
             bu.UserId, Username, bu.Region
      FROM UserStats
               JOIN UserInfo bu on bu.UserId = UserStats.UserId
      WHERE Banned = false
      ORDER BY RankNumber ASC) xx FULL JOIN UserStats bu on bu.UserId = xx.UserId
WHERE lower(xx.username) LIKE CONCAT('%',@Query,'%')
LIMIT {limit}
;
";
        return await db.SafeQueryAsync<LeaderBoardUser>(sql, new { Query = query });
    }

    public static async Task<Result<IEnumerable<LeaderBoardUser>, string>> LeaderBoardSearchUser(
        NpgsqlConnection db,
        long limit,
        string query,
        string countryNameShortInfo) {
        var sql = @$"
SELECT RankNumber,
       bu.UserId as UserId,
       Username,
       Region,
       OverallScore,
       OverallPlaycount,
       OverallSs,
       OverallS,
       OverallA,
       bu.OverallAccuracy
FROM (SELECT rank()
             OVER (ORDER BY OverallScore DESC, bu.LastLoginTime DESC) as RankNumber,
             bu.UserId, Username, bu.Region
      FROM UserStats
               JOIN UserInfo bu on bu.UserId = UserStats.UserId
      WHERE banned = false
      ORDER BY RankNumber ASC) xx FULL JOIN UserStats bu on bu.UserId = xx.UserId
WHERE lower(xx.username) LIKE CONCAT('%',@Query,'%')
AND Region = @Region
LIMIT {limit}
;
";
        return await db.SafeQueryAsync<LeaderBoardUser>(
            sql,
            new { Query = query, Region = countryNameShortInfo.ToUpper() }
        );
    }
}