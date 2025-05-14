using Dapper;
using LamLibAllOver;
using Npgsql;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryView_UserInfo_UserStats: IQueryView_UserInfo_UserStats {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryView_UserInfo_UserStats(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<Option<View_UserInfo_UserStats>>> GetByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<View_UserInfo_UserStats>(
            $"""
              SELECT * 
              FROM "View_UserInfo_UserStats"
              WHERE "UserId" = @UserId
             """, new { UserId = userId }
        )).LogIfError(Logger)
          .ToResultOk();
    }
}