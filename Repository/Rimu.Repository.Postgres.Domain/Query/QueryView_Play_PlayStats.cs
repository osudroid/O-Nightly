using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryView_Play_PlayStats: IQueryView_Play_PlayStats {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryView_Play_PlayStats(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryAsync<View_Play_PlayStats>(
            """
            SELECT * FROM "View_Play_PlayStats";
            """
        )).Map(x => x.ToList())
          .LogIfError(Logger)
          .ToResultOk();
    }
    
    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetUserTopScoreAsync(
        long userId,
        string filename,
        string fileHash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * FROM "View_Play_PlayStats" v 
                            WHERE v."UserId" = @UserId 
                              AND v."Filename" = @Filename 
                              AND v."FileHash" = @FileHash 
                            Order By "Score" DESC
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(
            sql,
            new { UserId = userId, Filename = filename, FileHash = fileHash }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetUserTopPpAsync(
        long userId,
        string filename,
        string fileHash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * FROM "View_Play_PlayStats" v 
                            WHERE v."UserId" = @UserId 
                              AND v."Filename" = @Filename 
                              AND v."FileHash" = @FileHash 
                            Order By "Pp" DESC
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(
                sql,
                new { UserId = userId, Filename = filename, FileHash = fileHash }
            )).LogIfError(Logger)
              .ToResultOk();
    }
    
    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetByIdAsync(long playId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $""" 
                   SELECT * FROM "View_Play_PlayStats" v WHERE v."Id" = {playId} LIMIT 1; 
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }
    
    public async Task<ResultOk<long>> GetUserMapRankAsync(long playId) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """
                           SELECT count(*) + 1 as Value 
                           FROM "View_Play_PlayStats" vpps,
                                (
                                    SELECT "Filename", "FileHash", "Pp" 
                                    FROM "View_Play_PlayStats" vpps2
                                    WHERE "Id" = @Id
                                ) refer
                           JOIN "UserInfo" ui on vpps."UserId" = ui."UserId"
                           WHERE ui."RestrictMode" = false
                             AND ui."Banned" = false
                             AND vpps."Filename" = refer."Filename"
                             AND vpps."FileHash" = refer."FileHash"
                             AND vpps."Pp" > refer."Pp"
                           """;
        
        return (await db.SafeQueryFirstAsync<BoxLong>(sql, new { Id = playId }))
        .LogIfError(Logger)
        .ToResultOk()
        .Map(x => x.Value)
        ;
    }


    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreByIdAsync(long id) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(
            $"""
             SELECT * 
             FROM "View_Play_PlayStats" v 
             WHERE v."Id" = {id}
             """
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreByIdAndUserIdAsync(long id, long userId) {

        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * FROM "View_Play_PlayStats" v WHERE v."Id" = {id} AND "UserId" = {userId}
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreOldesByUserIdAndHashAsync(long userId, string mapHash) {

        var db = await _dbContext.GetConnectionAsync();
        var sql = $""" 
                   SELECT * 
                   FROM "View_Play_PlayStats" v 
                   WHERE v."UserId" = {userId}
                   AND v."FileHash" = {mapHash}
                   ORDER BY "Date" DESC 
                   LIMIT 1 
                   """;
        
        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IReadOnlyList<Adapter.Class.MapTopPlays>>> MapTopPlaysByFilenameAndHashAsync(
        string filename,
        string fileHash,
        int maxResult) {
      
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT DISTINCT ON ("UserId") 
    x."Id" as "PlayScoreId", 
    x."UserId" as "UserId", 
    x."Mode" as "Mode", 
    x."Score" as "Score", 
    x."Combo" as "Combo", 
    x."Mark" as "Mark", 
    x."Date" as "Date", 
    x."Accuracy" as "Accuracy", 
    x."Username" as "Username"
FROM (SELECT 
          v."Id", 
          v."UserId",
          v."Mode", 
          v."Score", 
          v."Combo", 
          v."Mark", 
          v."Date", 
          v."Accuracy", 
          ur."Username"
      FROM "View_Play_PlayStats" v
          FULL JOIN "UserInfo" ur on v."UserId" = ur."UserId"
      WHERE v."Filename" = @Filename
        AND v."FileHash" = @Hash
      ORDER BY v."Score" DESC, v."Accuracy" DESC, v."Date" DESC 
      ) x
LIMIT {maxResult}
""";
        var result = (await db.SafeQueryAsync<MapTopPlays>(
            sql,
            new { Filename = filename, Hash = fileHash }
        )).Map(x => x.ToList())
          .LogIfError(Logger)
          .ToResultOk();

        if (result == EResult.Err) {
            return ResultOk<IReadOnlyList<MapTopPlays>>.Err();
        }

        var plays = result.Ok();

        for (var i = 0; i < plays.Count; i++) {
            plays[i].PlayRank = i + 1;
        }

        return ResultOk<IReadOnlyList<MapTopPlays>>.Ok(plays);
    }

    public async Task<ResultOk<IReadOnlyList<Adapter.Class.MapTopPlays>>> MapTopPlaysByFilenameAndHashAllAsync(
        string filename, 
        string fileHash) {
        
        var db = await _dbContext.GetConnectionAsync();
            var sql = $"""
    SELECT DISTINCT ON ("UserId") 
        "Id" as PlayScoreId, 
        "UserId" as UserId, 
        "Mode" as Mode, 
        "Score" as Score, 
        "Combo" as Combo, 
        "Mark" as Mark, 
        "Date" as Date, 
        "Accuracy" as Accuracy, 
        "Username" as Username
    FROM (SELECT 
              v."Id", 
              v."UserId",
              v."Mode", 
              v."Score", 
              v."Combo", 
              v."Mark", 
              v."Date", 
              v."Accuracy", 
              ur."Username"
          FROM "View_Play_PlayStats" v
              FULL JOIN "UserInfo" ur on v."UserId" = ur."UserId"
          WHERE v."Filename" = @Filename
            AND v."FileHash" = @Hash
          ORDER BY v."Score" DESC, v."Accuracy" DESC, v."Date" DESC 
          ) x
    """;
            var result = (await db.SafeQueryAsync<MapTopPlays>(
                sql,
                new { Filename = filename, Hash = fileHash }
            )).Map(x => x.ToList())
              .LogIfError(Logger)
              .ToResultOk();
    
            if (result == EResult.Err)
                return ResultOk<IReadOnlyList<MapTopPlays>>.Err();
    
            var plays = result.Ok();
    
            for (var i = 0; i < plays.Count; i++) {
                plays[i].PlayRank = i + 1;
            }
    
            return ResultOk<IReadOnlyList<MapTopPlays>>.Ok(plays);
        }
    
    public async Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdAsync(long userId, int limit, long offset) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * 
                   FROM (
                            SELECT distinct ON (v."Filename") * FROM "View_Play_PlayStats" v
                            WHERE v."UserId" = {userId}
                            ORDER BY v."Filename", v."Score" DESC
                        ) x
                   ORDER BY "Score" DESC
                   OFFSET {offset}
                   LIMIT {limit}
                   """; 
        
        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetLastPlayScoreFilterByUserIdAsync(long userId, int limit) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT * 
FROM "View_Play_PlayStats" v
WHERE v."UserId" = {userId}
ORDER BY v."Id" DESC
LIMIT {limit};
""";
        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdFilterMarkAsync(
        long userId, 
        int size, 
        long offset, 
        PlayStatsDto.EPlayScoreMark mark) {
        
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT *
FROM (
         SELECT distinct ON ("Filename") * 
         FROM "View_Play_PlayStats" v
         WHERE v."UserId" = {userId}
         AND v."Mark" = @Mark
         ORDER BY v."Filename", v."Score" DESC
     ) x
ORDER BY x."Score"
LIMIT {size}
OFFSET {offset}
""";
        
        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql, new { Mark = mark.ToString() }))
            .Map(x => x.ToList())
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdWithPageAsync(
        long userId,
        long page,
        int pageSize) {
        
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT *
FROM (
         SELECT distinct ON (v."Filename") * FROM "View_Play_PlayStats" v
         WHERE v."UserId" = {userId}
         ORDER BY v."Filename", v."Score" DESC
     ) x
ORDER BY x."Score"
LIMIT {pageSize}
OFFSET {page * pageSize}
""";

        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql))
               .Map(x => x.ToList())
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<Dictionary<PlayStatsDto.EPlayScoreMark, long>>> CountMarkPlaysByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT count(*) as "Count", b."New" As "NewMark"
FROM "View_Play_PlayStats" v
JOIN "OldMarkNewMark" b ON v."Mark" = b."Old" 
WHERE v."UserId" = {userId}
GROUP BY b."New"
""";
        
        var fetchMarkResult = (await db.SafeQueryAsync<CountMarkPlaysByUserIdClass>(sql))
            .LogIfError(Logger)
            .ToResultOk();
        if (fetchMarkResult == EResult.Err) {
            return ResultOk<Dictionary<PlayStatsDto.EPlayScoreMark, long>>.Err();
        }

        var res = new Dictionary<PlayStatsDto.EPlayScoreMark, long>(12);

        foreach (var row in fetchMarkResult.Ok()) {
            if (Enum.TryParse<PlayStatsDto.EPlayScoreMark>(row.NewMark, out _) == false) {
                return SResult<Dictionary<PlayStatsDto.EPlayScoreMark, long>>
                       .Err(TraceMsg.WithMessage($"String ({row.NewMark}) Parse To Entities.BblScore.EMark"))
                       .LogIfError(Logger)
                       .ToResultOk();
            }
            
            res[row.GetMarkAsEMark()] = row.Count;
        }

        return ResultOk<Dictionary<PlayStatsDto.EPlayScoreMark, long>>.Ok(res);
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreByHashAsync(string playHash, int limit) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                   SELECT *
                   FROM "View_Play_PlayStats" v
                   WHERE v."FileHash" = @PlayHash
                   ORDER BY v."Score"
                   LIMIT @Limit
                   """;

        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql, new { PlayHash = playHash, Limit = limit }))
               .Map(x => x.ToList())
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats>>> GetTopPpByHashAsync(string playHash, int limit) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats" v
                           WHERE v."FileHash" = @PlayHash
                           ORDER BY v."Pp"
                           LIMIT @Limit
                           """;

        return (await db.SafeQueryAsync<View_Play_PlayStats>(sql, new { PlayHash = playHash, Limit = limit }))
               .Map(x => x.ToList())
               .LogIfError(Logger)
               .ToResultOk();
    }

    private class CountMarkPlaysByUserIdClass {
        public long Count { get; }
        public string? NewMark { get; }

        public PlayStatsDto.EPlayScoreMark GetMarkAsEMark() {
            return Enum.TryParse(NewMark ?? "", out PlayStatsDto.EPlayScoreMark found)
                ? found
                : throw new Exception("EPlayScoreMark Not Found");
        }
    }

//     public static async Task<SResultErr> UpdateAsync(NpgsqlConnection db, View_Play_PlayStats playScoreNotuse) {
//         return (await db.SafeExecuteAsync(
//             $"""
//              Update PlayScore
//              SET Filename = @Filename, 
//                  Hash = @Hash, 
//                  Mode = @Mode, 
//                  Score = @Score, 
//                  Combo = @Combo, 
//                  Mark = @Mark, 
//                  Geki = @Geki, 
//                  Perfect = @Perfect, 
//                  Katu = @Katu, 
//                  Good = @Good, 
//                  Bad = @Bad, 
//                  Miss = @Miss, 
//                  Date = @Date, 
//                  Accuracy = @Accuracy  
//              WHERE  
//                  PlayScoreId = @PlayScoreId
//              """, playScoreNotuse));
//     }
}