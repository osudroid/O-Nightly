using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryUserStats: IQueryUserStats {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryUserStats(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<Option<UserStats>>> GetByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * 
                   FROM "UserStats"
                   WHERE "UserId" = @UserId
                   LIMIT 1
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserStats>(sql, new { UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk();
    }
    
    public async Task<ResultOk<long>> GetUserRank(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT UserRank as Value
FROM (
    SELECT 
        userStats."UserId", 
        rank() OVER ( ORDER BY "OverallScore" DESC, bu."LastLoginTime" DESC) as UserRank
    FROM "UserStats" userStats
    JOIN "UserInfo" bu on userStats."UserId" = bu."UserId"
    WHERE bu."Banned" = false
      AND bu."RestrictMode" = false
      AND bu."Archived" = false
    ) as t
WHERE "UserId" = {userId};
""";
        return (await db.SafeQueryFirstAsync<BoxLong>(sql))
               .LogIfError(Logger)
               .ToResultOk()
               .Map(x => x.Value)
            ;
    }

    public async Task<ResultNone> InsertAsync(UserStats userStats) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeInsertAsync(userStats))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> InsertBulkAsync(UserStats[] userStatsList) {
        var db = await _dbContext.GetConnectionAsync();
        try { 
            await db.ExecuteAsync(
                """
INSERT INTO "UserStats" 
    (
     "UserId", 
     "OverallPlaycount", 
     "OverallScore", 
     "OverallAccuracy", 
     "OverallCombo", 
     "OverallXss", 
     "OverallXs", 
     "OverallSs", 
     "OverallS", 
     "OverallA", 
     "OverallB", 
     "OverallC", 
     "OverallD", 
     "OverallPerfect", 
     "OverallHits", 
     "Overall300", 
     "Overall100", 
     "Overall50", 
     "OverallGeki", 
     "OverallKatu", 
     "OverallMiss",
     "OverallPp"
     )
VALUES (
        @UserId, 
        @OverallPlaycount, 
        @OverallScore, 
        @OverallAccuracy, 
        @OverallCombo, 
        @OverallXss, 
        @OverallXs, 
        @OverallSs, 
        @OverallS, 
        @OverallA, 
        @OverallB, 
        @OverallC, 
        @OverallD, 
        @OverallPerfect, 
        @OverallHits, 
        @Overall300, 
        @Overall100, 
        @Overall50, 
        @OverallGeki, 
        @OverallKatu, 
        @OverallMiss,
        @OverallPp
);
""", userStatsList
        );
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Trace(e);
            return ResultNone.Err;
        }
    }
    
    public async Task<ResultNone> InsertOrUpdateBulkAsync(UserStats[] userStatsList) {
        var db = await _dbContext.GetConnectionAsync();
        try { 
            await db.ExecuteAsync(
                """
                INSERT INTO "UserStats" 
                    (
                     "UserId", 
                     "OverallPlaycount", 
                     "OverallScore", 
                     "OverallAccuracy", 
                     "OverallCombo", 
                     "OverallXss", 
                     "OverallXs", 
                     "OverallSs", 
                     "OverallS", 
                     "OverallA", 
                     "OverallB", 
                     "OverallC", 
                     "OverallD", 
                     "OverallPerfect", 
                     "OverallHits", 
                     "Overall300", 
                     "Overall100", 
                     "Overall50", 
                     "OverallGeki", 
                     "OverallKatu", 
                     "OverallMiss",
                     "OverallPp"
                     )
                VALUES (
                        @UserId, 
                        @OverallPlaycount, 
                        @OverallScore, 
                        @OverallAccuracy, 
                        @OverallCombo, 
                        @OverallXss, 
                        @OverallXs, 
                        @OverallSs, 
                        @OverallS, 
                        @OverallA, 
                        @OverallB, 
                        @OverallC, 
                        @OverallD, 
                        @OverallPerfect, 
                        @OverallHits, 
                        @Overall300, 
                        @Overall100, 
                        @Overall50, 
                        @OverallGeki, 
                        @OverallKatu, 
                        @OverallMiss,
                        @OverallPp
                ) 
                ON CONFLICT ("UserId") DO UPDATE 
                SET 
                "OverallPlaycount" = @OverallPlaycount, 
                "OverallScore" = @OverallScore, 
                "OverallAccuracy" = @OverallAccuracy, 
                "OverallCombo" = @OverallCombo, 
                "OverallXss" = @OverallXss, 
                "OverallXs" = @OverallXs, 
                "OverallSs" = @OverallSs, 
                "OverallS" = @OverallS, 
                "OverallA" = @OverallA, 
                "OverallB" = @OverallB, 
                "OverallC" = @OverallC, 
                "OverallD" = @OverallD, 
                "OverallPerfect" = @OverallPerfect, 
                "OverallHits" = @OverallHits, 
                "Overall300" = @Overall300, 
                "Overall100" = @Overall100, 
                "Overall50" = @Overall50, 
                "OverallGeki" = @OverallGeki, 
                "OverallKatu" = @OverallKatu, 
                "OverallMiss" = @OverallMiss,
                "OverallPp" = @OverallPp
                ;
                """, userStatsList
            );
            
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Trace(e, "InsertOrUpdateBulkAsync");
            return ResultNone.Err;
        }
    }
    
    public async Task<ResultNone> UpdateStatsFromPlayStatsAsync(long userId, PlayStatsDto now, PlayStatsDto? old = null) {
        var db = await _dbContext.GetConnectionAsync();
        PlayStatsDto dif = old.HasValue ? now - old.Value: now;
        
        var playcount = old.HasValue? 0: 1;

        var sql = $"""
                   UPDATE "UserStats" 
                   SET
                       "OverallPlaycount" = "OverallPlaycount" + {playcount}, 
                       "OverallScore" = "OverallScore" + {dif.Score}, 
                       "OverallAccuracy" = "OverallAccuracy" + {dif.Accuracy}, 
                       "OverallCombo" = "OverallCombo" + {dif.Combo}, 
                       "OverallXss" = "OverallXss" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.XSS)}, 
                       "OverallSs" = "OverallSs" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.SS)}, 
                       "OverallXs" = "OverallXs" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.XS)}, 
                       "OverallS" = "OverallS" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.S)}, 
                       "OverallA" = "OverallA" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.A)}, 
                       "OverallB" = "OverallB" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.B)}, 
                       "OverallC" = "OverallC" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.C)}, 
                       "OverallD" = "OverallD" + {dif.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.D)}, 
                       "OverallHits" = "OverallHits" + {dif.GetValue(PlayStatsDto.EPlayScore.Hits)},  
                       "Overall300" = "Overall300" + {dif.GetValue(PlayStatsDto.EPlayScore.N300)}, 
                       "Overall100" = "Overall100" + {dif.GetValue(PlayStatsDto.EPlayScore.N100)}, 
                       "Overall50" = "Overall50" + {dif.GetValue(PlayStatsDto.EPlayScore.N50)}, 
                       "OverallGeki" = "OverallGeki" + {dif.Geki}, 
                       "OverallKatu" = "OverallKatu" + {dif.Katu}, 
                       "OverallMiss" = "OverallMiss" + {dif.Miss},
                       "OverallPp" = "OverallPp" + {dif.Pp}
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryAsync(sql))
            .LogIfError(Logger)
            .ToNone();
    }


    public async Task<ResultOk<Option<UserStats>>> GetBblUserStatsByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * 
                   FROM "UserStats"
                   WHERE "UserId" = {userId}
                   LIMIT 1
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserStats>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardFilterCountry(int limit, string countryNameShort) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT rank() OVER (ORDER BY "OverallPp" DESC, bu."LastLoginTime" DESC) as "RankNumber", 
       bu."UserId" as "UserId",
       "Username",
       "Region",
       "OverallScore",
       "OverallPlaycount",
       "OverallSs",
       "OverallS",
       "OverallA",
       "OverallAccuracy",
       "OverallPp"
FROM "UserStats"
         JOIN "UserInfo" bu on bu."UserId" = "UserStats"."UserId"
WHERE "Banned" = false AND bu."Region" = @Region
ORDER BY "RankNumber"
LIMIT {limit}
""";
        return (await db.SafeQueryAsync<LeaderBoardUser>(sql, new { Region = countryNameShort }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<LeaderBoardUser>>> LeaderBoardUserRank(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT xx."UserId",
       "RankNumber",
       "Username",
       "Region",
       "OverallScore",
       "OverallPlaycount",
       "OverallSs",
       "OverallS",
       "OverallA",
       "OverallPp",
       bu."OverallAccuracy"
FROM (SELECT rank()
             OVER (ORDER BY "OverallPp" DESC, bu."LastLoginTime" DESC) as "RankNumber",
             bu."UserId", "Username", bu."Region"
      FROM "UserStats"
               JOIN "UserInfo" bu on bu."UserId" = "UserStats"."UserId"
      WHERE "Banned" = false
      ORDER BY "RankNumber" ASC) xx FULL JOIN "UserStats" bu on bu."UserId" = xx."UserId"
WHERE xx."UserId" = @0
""";
        return (await db.SafeQueryFirstOrDefaultAsync<LeaderBoardUser>(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardNoFilter(int limit) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT rank() 
OVER (ORDER BY "OverallPp" DESC, "UserInfo"."UserId" DESC) as "RankNumber",
       "UserInfo"."UserId" as "UserId",
       "Username",
       "Region",
       "OverallScore",
       "OverallPlaycount",
       "OverallSs",
       "OverallS",
       "OverallA",
       "OverallAccuracy",
       "OverallPp"
FROM "UserInfo" 
         JOIN "UserStats" bu on bu."UserId" = "UserInfo"."UserId"
WHERE "Banned" = false
ORDER BY "OverallPp" DESC, "UserInfo"."UserId" DESC
LIMIT {limit};
""";
        return (await db.SafeQueryAsync<LeaderBoardUser>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardSearchUser(long limit, string query) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT "RankNumber",
       bu."UserId" as "UserId",
       "Username",
       "Region",
       "OverallScore",
       "OverallPlaycount",
       "OverallSs",
       "OverallS",
       "OverallA",
       bu."OverallAccuracy",
       bu."OverallPp"
FROM (SELECT rank()
             OVER (ORDER BY "OverallScore" DESC, bu."LastLoginTime" DESC) as "RankNumber",
             bu."UserId", "Username", bu."Region"
      FROM "UserStats"
               JOIN "UserInfo" bu on bu."UserId" = "UserStats"."UserId"
      WHERE "Banned" = false
      ORDER BY "OverallScore" DESC, bu."LastLoginTime" DESC) xx FULL JOIN "UserStats" bu on bu."UserId" = xx."UserId"
WHERE lower(xx."Username") LIKE CONCAT('%',@Query,'%')
LIMIT {limit}
""";
        return (await db.SafeQueryAsync<LeaderBoardUser>(sql, new { Query = query }))
            .LogIfError(Logger)
            .ToResultOk();
    }
    
    public async Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardSearchUser(
        long limit,
        string query,
        string countryNameShortInfo) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT "RankNumber",
       bu."UserId" as UserId,
       "Username",
       "Region",
       "OverallScore",
       "OverallPlaycount",
       "OverallSs",
       "OverallS",
       "OverallA",
       "OverallPp",
       bu."OverallAccuracy"
FROM (SELECT rank()
             OVER (ORDER BY "OverallScore" DESC, bu."LastLoginTime" DESC) as "RankNumber",
             bu."UserId", "Username", bu."Region"
      FROM "UserStats"
               JOIN "UserInfo" bu on bu."UserId" = "UserStats"."UserId"
      WHERE "Banned" = false
      ORDER BY "OverallScore" DESC, bu."LastLoginTime" DESC) xx FULL JOIN "UserStats" bu on bu."UserId" = xx."UserId"
WHERE lower(xx."Username") LIKE CONCAT('%',@Query,'%')
AND "Region" = @Region
LIMIT {limit}
;
""";
        return (await db.SafeQueryAsync<LeaderBoardUser>(
            sql,
            new { Query = query, Region = countryNameShortInfo.ToUpper() }
        )).LogIfError(Logger)
          .ToResultOk();
    }
    
    public async Task<ResultOk<Option<long>>> UserRankByUserIdScopeCountryAsync(
        long userId,
        string countryNameShortInfo) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT "RankNumber"
                   FROM 
                       (
                       SELECT 
                           rank() OVER (ORDER BY "OverallPp" DESC, bu."LastLoginTime" DESC) as "RankNumber",
                           bu."UserId", bu."Region"
                       FROM "UserStats"
                           JOIN "UserInfo" bu on bu."UserId" = "UserStats"."UserId"
                       WHERE "Banned" = false
                       ORDER BY "OverallPp" DESC, bu."LastLoginTime" DESC
                     ) xx FULL JOIN "UserStats" bu on bu."UserId" = xx."UserId"
                   WHERE bu."UserId" = @UserId
                   AND "Region" = @Region
                   ;
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<LeaderBoardUser>(
                sql,
                new { UserId = userId, Region = countryNameShortInfo.ToUpper() }
            )).LogIfError(Logger)
              .ToResultOk()
              .Map(x => x.Map(x => x.RankNumber));
    }
}