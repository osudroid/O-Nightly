using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryUserSetting: IQueryUserSetting {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryUserSetting(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultOk<Option<UserSetting>>> GetByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           SELECT * 
                           FROM "UserSetting"
                           WHERE "UserId" = @UserId
                           """;
        
        return (await db.SafeQueryFirstOrDefaultAsync<UserSetting>(sql, new { UserId = userId }))
            .LogIfError(Logger);
    }

    public async Task<ResultNone> InsertAsync(UserSetting userSetting) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           INSERT INTO "UserSetting" 
                               ("UserId", "ShowUserClassifications")
                           VALUES
                               (@UserId, @ShowUserClassifications) 
                           """;
        
        return (await db.SafeExecuteAsync(sql, userSetting))
               .LogIfError(Logger)
               .ToNone()
        ;
    }

    public async Task<ResultNone> DeleteAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           DELETE FROM "UserSetting"
                           WHERE "UserId" = @UserId
                           """;
        
        return (await db.SafeExecuteAsync(sql, new { UserId = userId }))
               .LogIfError(Logger)
               .ToNone()
            ;
    }

    public async Task<ResultNone> UpdateAsync(UserSetting userSetting) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                           UPDATE "UserSetting"
                           SET "ShowUserClassifications" = @ShowUserClassifications
                           WHERE "UserId" = @UserId 
                           """;
        return (await db.SafeExecuteAsync(sql, userSetting))
               .LogIfError(Logger)
               .ToNone()
            ;
    }
}