using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryUserAvatar: IQueryUserAvatar {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryUserAvatar(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultNone> InsertAsync(UserAvatar userAvatar) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeInsertAsync(userAvatar))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> DeleteByUserIdAndHash(long userId, string hash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  DELETE 
                  FROM "UserAvatar" 
                  WHERE "UserId" = @UserId
                  AND "Hash" = @Hash
                  """;
        
        return (await db.SafeQueryAsync(sql, new { UserId = userId, Hash = hash }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> DeleteAllFromUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  DELETE 
                  FROM "UserAvatar" 
                  WHERE "UserId" = @UserId
                  """;
        return (await db.SafeQueryAsync(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetByUserIdAndSizeNoOriginalAsync(long userId, int size) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT * 
FROM "UserAvatar"
WHERE "UserId" = @UserId
AND "PixelSize" = @PixelSize
AND "Original" = false
""";

        return 
            (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql, new { UserId = userId, PixelSize = size }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetByHashAsync(string hash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT * 
FROM "UserAvatar"
WHERE "Hash" = @Hash
""";

        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql, new { Hash = hash }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetOriginalByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * 
                   FROM "UserAvatar"
                   WHERE "UserId" = {userId}
                   AND "Original" = true
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetLowByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT * 
FROM "UserAvatar"
WHERE "UserId" = {userId}
AND "Original" = false
ORDER BY "PixelSize" ASC 
""";
        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetHighByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
SELECT * 
FROM "UserAvatar"
WHERE "UserId" = {userId}
AND "Original" = false
ORDER BY "PixelSize" DESC 
""";
        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserAvatar>>> GetByUserIdAndHash(long userId, string hash) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = $"""
                   SELECT * 
                   FROM "UserAvatar"
                   WHERE "UserId" = @UserId
                     AND "Hash" = @Hash
                   ORDER BY "PixelSize" DESC 
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql, new { UserId = userId, Hash = hash }))
               .LogIfError(Logger)
               .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<UserAvatar>>> GetManyUserIdAndHashByPixelSizeUserIdAsync(
        int pixelSize,
        IReadOnlyList<long> userIds) {
        
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT "UserId", "Hash"
                   FROM "UserAvatar"
                   WHERE "PixelSize" = {pixelSize}
                   AND "UserId" in @UserIds
                   """;
        return (await db.SafeQueryAsync<UserAvatar>(sql, new { UserIds = userIds }))
            .LogIfError(Logger)
            .ToResultOk();
    }
}