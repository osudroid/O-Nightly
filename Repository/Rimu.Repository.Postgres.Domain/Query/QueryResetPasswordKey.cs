using System.Data.Common;
using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryResetPasswordKey: IQueryResetPasswordKey {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryResetPasswordKey(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultNone> InsertAsync(ResetPasswordKey entity) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync("""
                                          INSERT INTO "ResetPasswordKey"
                                              ("Token", "UserId", "CreateTime") 
                                          VALUES 
                                              (@Token, @UserId, @CreateTime); 
                                          """, new {
                entity.Token,
                entity.UserId,
                entity.CreateTime
            }
        )).LogIfError(Logger).ToNone();
    }

    public async Task<ResultOk<Option<ResetPasswordKey>>> FindByTokenAndUserId(string tokenId, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<ResetPasswordKey>(
            """ 
            SELECT * FROM "ResetPasswordKey" 
                     WHERE "Token" = @Token AND "UserId" = @UserId 
                     LIMIT 1
            """, new { Token = tokenId, UserId = userId }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<Option<ResetPasswordKey>>> FindByTokenAndUserIdFilterTimeGreaterOrEqual(
        string tokenId,
        long userId,
        DateTime createTime) {
        
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<ResetPasswordKey>(
            """
            SELECT * 
            FROM "ResetPasswordKey" 
            WHERE "Token" = @Token AND "UserId" = @UserId AND "CreateTime" <= @CreateTime LIMIT 1
            """, new { Token = tokenId, UserId = userId, CreateTime = createTime }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultNone> DeleteByTokenAndUserId(string tokenId, long userId) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
            """
            Delete FROM "ResetPasswordKey" 
                   WHERE "Token" = @Token AND "UserId" = @UserId
            """, new { Token = tokenId, UserId = userId }
        )).LogIfError(Logger)
          .ToNone();
    }

    public async Task<ResultNone> DeleteAllOlderThenCreateTime(DateTime createTime) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeExecuteAsync(
            """
            Delete FROM "ResetPasswordKey" WHERE "CreateTime" > @CreateTime
            """, new { CreateTime = createTime }
        )).LogIfError(Logger)
          .ToNone();
    }
}