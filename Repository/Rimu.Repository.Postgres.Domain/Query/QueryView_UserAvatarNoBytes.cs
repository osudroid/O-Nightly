using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public sealed class QueryView_UserAvatarNoBytes: IQueryView_UserAvatarNoBytes {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IDbContext _dbContext;

    public QueryView_UserAvatarNoBytes(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<View_UserAvatarNoBytes[]>> FindByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """
                           SELECT * 
                           FROM "View_UserAvatarNoBytes"
                           WHERE "UserId" = @UserId 
                           """;
        
        return (await db.SafeQueryAsync<View_UserAvatarNoBytes>(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .Map(static x => x.ToArray());
    }

    public async Task<ResultOk<Option<View_UserAvatarNoBytes>>> FindByUserIdAndHashAsync(long userId, string hash) {
        var db = await _dbContext.GetConnectionAsync();

        const string sql = """
                           SELECT * 
                           FROM "View_UserAvatarNoBytes"
                           WHERE "UserId" = @UserId
                             AND "Hash" = @Hash
                           """;
        
        return (await db.SafeQueryFirstOrDefaultAsync<View_UserAvatarNoBytes>(sql, new { UserId = userId }))
               .LogIfError(Logger)
               ;
    }
}