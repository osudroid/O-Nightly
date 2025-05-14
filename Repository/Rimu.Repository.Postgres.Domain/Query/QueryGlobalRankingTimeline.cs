using System.Data;
using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryGlobalRankingTimeline: IQueryGlobalRankingTimeline {
    private readonly IDbContext _dbContext;
    private readonly IQuery _query;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryGlobalRankingTimeline(IDbContext dbContext, IQuery query) {
        _dbContext = dbContext;
        _query = query;
    }

    public async Task<ResultOk<List<GlobalRankingTimeline>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        
        var sql = """
                  SELECT * FROM "GlobalRankingTimeline" 
                  """;
        return (await db.SafeQueryAsync<GlobalRankingTimeline>(sql))
            .LogIfError(Logger)
            .Map(x => x.ToList());
    }
    
    public async Task<ResultOk<Option<GlobalRankingTimeline>>> Now(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var resultLeaderBoardUser = (await _query.LeaderBoardUserSingleUser(userId));

        if (resultLeaderBoardUser == EResult.Err) {
            return ResultOk<Option<GlobalRankingTimeline>>.Err();
        }

        var leaderBoardUser = resultLeaderBoardUser.Ok();

        if (leaderBoardUser.IsSet())
            return ResultOk<Option<GlobalRankingTimeline>>
                .Ok(Option<GlobalRankingTimeline>
                    .With(GlobalRankingTimeline.FromLeaderBoardUser(leaderBoardUser.Unwrap(), DateTime.UtcNow))
                );

        return ResultOk<Option<GlobalRankingTimeline>>.Ok(Option<GlobalRankingTimeline>.Empty);
    }

    public async Task<ResultOk<IReadOnlyList<GlobalRankingTimeline>>> BuildTimeLineAsync(long userId, DateTime startAt) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
            SELECT * 
            FROM "GlobalRankingTimeline"
            WHERE "UserId" = @UserId
            AND "Date" >= @StartAt
            ORDER BY "Date" 
        """;

        var resultFetch = (await db
                              .SafeQueryAsync<GlobalRankingTimeline>(sql, new { StartAt = startAt, UserId = userId }))
            .LogIfError(Logger)
            .ToResultOk();

        if (resultFetch == EResult.Err) {
            return ResultOk<IReadOnlyList<GlobalRankingTimeline>>.Err();
        }
        
        var rankingTimelines = resultFetch.Ok().ToList();
        var resList = rankingTimelines.Count == 0
            ? new List<GlobalRankingTimeline>(0)
            : rankingTimelines;

        var resultNow = (await Now(userId));
        if (resultNow == EResult.Err) {
            return ResultOk<IReadOnlyList<GlobalRankingTimeline>>.Err();
        }
        
        var optionNow = resultNow.Ok();
        if (optionNow.IsSet()) {
            resList.Add(optionNow.Unwrap());
        }
        return ResultOk<IReadOnlyList<GlobalRankingTimeline>>.Ok(resList);
    }

    public async Task<ResultNone> DeleteAllRankingByAllUserId(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """ 
Delete FROM "GlobalRankingTimeline" WHERE "UserId" = @UserId; 
""";
        return (await db.SafeQueryAsync(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> InsertBulkAsync(GlobalRankingTimeline[] globalRankingTimelines) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = 
            """
            INSERT INTO "GlobalRankingTimeline" 
                ("UserId", "Date", "GlobalRanking", "Pp") 
            VALUES
                (@UserId, @Date, @GlobalRanking, @Pp)
            """;

        return (await db.SafeExecuteAsync(sql, globalRankingTimelines))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultOk<GlobalRankingTimeline[]>> GetRangeByUserIdAndStartAtAsync(long userId, DateOnly startAt) {
        var db = await _dbContext.GetConnectionAsync();
        
        const string sql = 
            """
            SELECT *
            FROM "GlobalRankingTimeline"
            WHERE "UserId" = @UserId
              AND "Date" >= @StartAt
            
            """;
        
        return (await db.SafeQueryAsync<GlobalRankingTimeline>(sql, new { UserId = userId, StartAt = startAt }))
            .LogIfError(Logger)
            .Map(x => x.ToArray())
            ;
    }
}