using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryTokenUser: IQueryTokenUser {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryTokenUser(IDbContext dbContext) => _dbContext = dbContext;

    public async Task<ResultOk<IEnumerable<TokenUser>>> GetAllTokensAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryAsync<TokenUser>(""" SELECT * FROM "TokenUser" """))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultNone> CreateOrUpdateAsync(DateTime createDay, long userId, string token) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
INSERT 
INTO "TokenUser" ("TokenId", "UserId", "CreateDate") 
VALUES (@token, {userId}, @createDay) 
ON CONFLICT (@token) DO UPDATE 
set "CreateDate" = @createDay,
    "UserId" = {userId}
""";
        return (await db.SafeQueryAsync(sql, new { createDay, token }))
            .LogIfError(Logger)
            .ToNone();
    }

    public Task<ResultNone> CreateOrUpdateAsync(TokenUser tokenUser) {
        return CreateOrUpdateAsync(tokenUser.CreateDate, tokenUser.UserId, tokenUser.TokenId);
    }

    public async Task<ResultOk<Option<TokenUser>>> GetByTokenAsync(string tokenId) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = $"""
                   SELECT *  
                   FROM "TokenUser" 
                   WHERE "TokenId" = @TokenId
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<TokenUser>(sql, new { TokenId = tokenId }))
            .LogIfError(Logger)
        ;
    }

    public async Task<ResultOk<Option<TokenUser>>> GetByTokenAsync(Guid token) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                   SELECT * 
                   FROM "TokenUser"
                   WHERE "TokenId" = @id
                   """;
        
        return (await db.SafeQueryFirstOrDefaultAsync<TokenUser>(sql, new { id = token }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultNone> DeleteOlderEqThen(DateTime time) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   DELETE 
                   FROM "TokenUser"
                   WHERE "CreateDate" <= '{Time.ToScyllaString(time)}'
                   """;
        return (await db.SafeQueryAsync<TokenUser>(sql)).LogIfError(Logger).ToNone();
    }

    public async Task<ResultNone> DeleteManyByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  DELETE 
                  FROM "TokenUser"
                  WHERE "UserId" = @UserId
                  """;
        return (await db.SafeQueryAsync<TokenUser>(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> DeleteByTokenIdAsync(string tokenId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  DELETE 
                  FROM "TokenUser"
                  WHERE "TokenId" = @TokenId
                  """;
        return (await db.SafeQueryAsync<TokenUser>(sql, new { TokenId = tokenId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> UpdateCreateTimeAsync(string token, DateTime createDate) {
        var db = await _dbContext.GetConnectionAsync();

        var sql = $"""
                   UPDATE "TokenUser"
                   SET "CreateDate" = '{Time.ToScyllaString(createDate)}'
                   WHERE "TokenId" = @Token 
                   """;
        return (await db.SafeQueryAsync(sql, new { Token = token }))
            .LogIfError(Logger)
            .ToNone();
    }
}