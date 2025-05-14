using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryView_Play_PlayStats_UserInfo: IQueryView_Play_PlayStats_UserInfo {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryView_Play_PlayStats_UserInfo(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<View_Play_PlayStats_UserInfo[]>> BeatmapTopAsync(string filename, string filehash, long offset, int length) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats_UserInfo"
                           WHERE "Filename" = @Filename
                             AND "FileHash" = @FileHash
                           ORDER BY "Pp" DESC
                           OFFSET @offset
                           LIMIT @length
                           """;

        return (await db.SafeQueryAsync<View_Play_PlayStats_UserInfo>(
            sql,
            new {
                Filename = filename,
                FileHash = filehash,
                offset = offset,
                length = length,
            }
        )).LogIfError(Logger)
          .Map(static x => x.ToArray());
    }

    public async Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> GetTopScoreByHashAsync(string playHash, int limit) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats_UserInfo" v
                           WHERE v."FileHash" = @PlayHash
                           ORDER BY v."Score"
                           LIMIT @Limit
                           """;

        return (await db.SafeQueryAsync<View_Play_PlayStats_UserInfo>(sql, new { PlayHash = playHash, Limit = limit }))
               .Map(x => x.ToList())
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> GetTopPpByHashAsync(string playHash, int limit) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats_UserInfo" v
                           WHERE v."FileHash" = @PlayHash
                           ORDER BY v."Pp"
                           LIMIT @Limit
                           """;

        return (await db.SafeQueryAsync<View_Play_PlayStats_UserInfo>(sql, new { PlayHash = playHash, Limit = limit }))
               .Map(x => x.ToList())
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<Option<View_Play_PlayStats_UserInfo>>> GetTopPpByHashAndUserIdAsync(string playHash, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats_UserInfo" v
                           WHERE v."FileHash" = @PlayHash
                             AND v."UserId" = @UserId
                           ORDER BY v."Pp"
                           LIMIT 1
                           """;

        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats_UserInfo>(sql, new { PlayHash = playHash, UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk();
    }
    
    public async Task<ResultOk<Option<View_Play_PlayStats_UserInfo>>> GetTopScoreByHashAndUserIdAsync(string playHash, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT *
                           FROM "View_Play_PlayStats_UserInfo" v
                           WHERE v."FileHash" = @PlayHash
                             AND v."UserId" = @UserId
                           ORDER BY v."Score"
                           LIMIT 1
                           """;

        return (await db.SafeQueryFirstOrDefaultAsync<View_Play_PlayStats_UserInfo>(sql, new { PlayHash = playHash, UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<Option<BoxLong>>> GetRankSortPpByHashAndUserIdAsync(string playHash, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT q.rank as "Value"
                           FROM
                               (
                               SELECT v."UserId", rank() OVER (ORDER BY v."Score" DESC) as rank
                               FROM "View_Play_PlayStats_UserInfo" v
                               WHERE v."FileHash" = @PlayHash
                                 AND v."UserId" = @UserId
                               ORDER by rank
                           ) q
                           WHERE (q."UserId" = @UserId)
                           LIMIT 1
                           """;

        return (await db.SafeQueryFirstOrDefaultAsync<BoxLong>(sql, new { PlayHash = playHash, UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk()
               ;
    }

    public async Task<ResultOk<Option<BoxLong>>> GetRankScoreSortByHashAndUserIdAsync(string playHash, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT q.rank as "Value"
                           FROM
                               (
                               SELECT v."UserId", rank() OVER (ORDER BY v."Pp" DESC) as rank
                               FROM "View_Play_PlayStats_UserInfo" v
                               WHERE v."FileHash" = @PlayHash
                                 AND v."UserId" = @UserId
                               ORDER by rank
                           ) q
                           WHERE (q."UserId" = @UserId)
                           LIMIT 1
                           """;

        return (await db.SafeQueryFirstOrDefaultAsync<BoxLong>(sql, new { PlayHash = playHash, UserId = userId }))
               .LogIfError(Logger)
               .ToResultOk()
            ;
    }
}