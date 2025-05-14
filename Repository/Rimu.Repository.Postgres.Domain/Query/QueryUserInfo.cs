using Dapper;
using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Postgres.Domain.Query;

public class QueryUserInfo: IQueryUserInfo {
    private readonly IDbContext _dbContext;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public QueryUserInfo(IDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<ResultNone> InsertAsync(UserInfo userInfo) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeInsertAsync(userInfo))
            .LogIfError(Logger)
            .ToNone();
    }
    
    public async Task<ResultOk<long>> GetNewUserIdAsync() {
        var db = await _dbContext.GetConnectionAsync();
        
        const string sql = 
                """
                SELECT nextval('bbl_user_id_seq') as value
                """
            ;

        return (await db.SafeQueryFirstAsync<long>(sql)).LogIfError(Logger);

    }

    public async Task<ResultNone> InsertBulkAsync(UserInfo[] userInfo) {
        var db = await _dbContext.GetConnectionAsync();
        var query = 
            """
            INSERT INTO "UserInfo" (
                "UserId",
                "Username",
                "Password",
                "Email",
                "DeviceId",
                "RegisterTime",
                "LastLoginTime",
                "Region",
                "Active",
                "Banned",
                "Archived",
                "RestrictMode",
                "UsernameLastChange",
                "LatestIp",
                "PatronEmail",
                "PatronEmailAccept"
            ) VALUES (
                @UserId,
                @Username,
                @Password,
                @Email,
                @DeviceId,
                @RegisterTime,
                @LastLoginTime,
                @Region,
                @Active,
                @Archived,
                @Banned,
                @RestrictMode,
                @UsernameLastChange,
                @LatestIp,
                @PatronEmail,
                @PatronEmailAccept
            );
            """
        ;

        return (await db.SafeExecuteAsync(query, userInfo))
               .LogIfError(Logger)
               .ToNone();
    }

    public async Task<ResultOk<long[]>> GetAllIdsAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryAsync<long>(
            """
            SELECT "UserId" FROM "UserInfo";    
            """
        )).Map(x => x.ToArray())
          .LogIfError(Logger)
          .ToResultOk();
    }
    
    public async Task<ResultOk<List<UserInfo>>> GetAllAsync() {
        var db = await _dbContext.GetConnectionAsync();
        return (
            await db.SafeQueryAsync<UserInfo>(
            """ SELECT * FROM "UserInfo" """
        )).Map(x => x.ToList())
          .LogIfError(Logger)
          .ToResultOk();
    }
    
    public async Task<ResultOk<Option<UserInfo>>> GetUserIdByUsernameAsync(string username) {
        var db = await _dbContext.GetConnectionAsync();
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(
            """ SELECT "UserId" FROM "UserInfo" WHERE "Username" = lower(@Username) LIMIT 1 """,
            new { Username = username }
        )).LogIfError(Logger)
          .ToResultOk();
    }

    public async Task<ResultOk<StatisticActiveUser>> GetStatisticActiveUserAsync() {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT 
    count(*) as register_user,
    sum(CASE WHEN "UserInfo"."LastLoginTime" >= @LLT0 THEN 1 ELSE 0 END) as "ActiveUserLast1H",
    sum(CASE WHEN "UserInfo"."LastLoginTime" >= @LLT1 THEN 1 ELSE 0 END) as "ActiveUserLast1Day"
FROM "UserInfo" 
WHERE "Banned" = false 
""";
        return (await db.SafeQueryFirstOrDefaultAsync<StatisticActiveUser>(
                sql,
                new { LLT0 = DateTime.UtcNow - TimeSpan.FromHours(1), LLT1 = DateTime.UtcNow - TimeSpan.FromDays(1) }
            )).LogIfError(Logger)
              .ToResultOk()
              .Map(x => x
                  .Or(new StatisticActiveUser() { ActiveUserLast1Day = 0, ActiveUserLast1H = 0, RegisterUser = 0})
              );
    }

    /// <summary> SELECT * FROM UserInfo </summary>
    public async Task<ResultOk<Option<UserInfo>>> GetByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT * 
                   FROM "UserInfo"
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserInfo>>> GetByUsernameAsync(string username) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT * 
                  FROM "UserInfo"
                  WHERE "Username" = @Username
                  """;        
        
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { Username = username }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserInfo>>> GetByEmailAsync(string email) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT * 
                  FROM "UserInfo"
                  WHERE "Email" = lower(@Email)
                  """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { Email = email }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserInfo>>> GetUsernameAndRegionByUserId(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT "Username", "Region"
                   FROM "UserInfo"
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<Option<UserInfo>>> GetUsernameByUserIdAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   SELECT "Username"
                   FROM "UserInfo"
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultNone> DeleteAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $""" 
                   DELETE 
                   FROM "UserInfo" 
                   WHERE "UserId" = @UserId
                   """;
        return (await db.SafeQueryAsync(sql, new { UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultOk<Option<UserInfo>>> GetIdUsernamePasswordByLowerUsernameAsync(string username) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT "UserId", "Username", "Password" 
                  FROM "UserInfo" 
                  WHERE lower("Username") = lower(@Username)
                  """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { Username = username }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    /// <returns> UserId, Email, Password, Username  </returns>
    public async Task<ResultOk<Option<UserInfo>>> GetLoginInfoByEmailAsync(string email) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT "UserId", "Email", "Password", "Username" 
                  FROM "UserInfo" 
                  WHERE "Email" = lower(@Email) 
                    AND "Banned" = false 
                  LIMIT 1
                  """;
        
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { Email = email }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    /// <returns> UserId, Email, Password, Username  </returns>
    public async Task<ResultOk<Option<UserInfo>>> GetLoginInfoByByUsernameAsync(string username) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  SELECT "UserId", "Email", "Password", "Username"
                  FROM "UserInfo" 
                  WHERE "Username" = lower(@Username) 
                    AND "Banned" = false 
                  LIMIT 1
                  """;
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { Username = username }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultNone> UpdateIpAndRegionAsync(long userId, string region, string latestIp) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
                  UPDATE "UserInfo"
                  SET "Region" = @Region, "LatestIp" = @LatestIp
                  WHERE "UserId" = @UserId 
                  """;
        
        return (await db.SafeQueryAsync(sql, new { Region = region, LatestIp = latestIp, UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> UpdateEmailByUserId(long userId, string email) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $""" 
                   UPDATE "UserInfo" 
                   SET "Email" = lower(@Email) 
                   WHERE "UserId" = @UserId
                   """;
        return (await db.SafeQueryAsync(sql, new { Email = email, UserId = userId }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> UpdatePasswordByUserIdAsync(long userId, string password) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
                   UPDATE "UserInfo" 
                   SET "Password" = lower(@Password) 
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryAsync(sql, new { Password = password }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> UpdateUsernameByUserIdAsync(long userId, string username) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $""" 
                   UPDATE "UserInfo" 
                   SET "Username" = lower(@Username) 
                   WHERE "UserId" = {userId}
                   """;
        return (await db.SafeQueryAsync(sql, new { Username = username }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultOk<Option<UserInfo>>> CheckPasswordGetIdAndUsernameAsync(string passwordHash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT "UserId", "Username"
FROM "UserInfo"
WHERE "Password" = @PasswordHash
""";
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql, new { PasswordHash = passwordHash }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<IEnumerable<UserInfo>>> GetEmailAndUsernameByEmailAndUsername(string email, string username) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
SELECT "Username", "Email" 
FROM "UserInfo"
WHERE "Email" = lower(@Email)
   Or "Username" = lower(@Username)
""";

        return (await db.SafeQueryAsync<UserInfo>(sql, new { Username = username, Email = email }))
            .LogIfError(Logger)
            .ToResultOk();
    }

    public async Task<ResultOk<bool>> CheckExistByEmailAndUsername(string email, string username) {
        var db = await _dbContext.GetConnectionAsync();
        return (await GetEmailAndUsernameByEmailAndUsername(email, username))
            .Map(x => x.Any());
    }

    public async Task<ResultNone> UpdatePasswordAsync(long userId, string passwordGen1Hash, string passwordGen2Hash) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = """
UPDATE "UserInfo"
SET "Password" = @Password,
    "PasswordGen2" = @PasswordGen2
WHERE "UserId" = @UserId 
""";
        return (await db.SafeQueryAsync(sql, 
                   new { Password = passwordGen1Hash, PasswordGen2 = passwordGen2Hash, UserId = userId }))
               .LogIfError(Logger)
               .ToNone();
    }

    public async Task<ResultOk<DateTime>> UpdateLastLoginTimeAsync(long userId) {
        var db = await _dbContext.GetConnectionAsync();
        var time = DateTime.UtcNow;
        var sql = $""" 
                   UPDATE "UserInfo" SET "LastLoginTime" = @Time WHERE "UserId" = {userId}
                   """;
        
        return (await db.SafeQueryAsync(sql, new { Time = time }))
               .LogIfError(Logger)
               .ToResultOk()
               .Map(x => time)
            ;
    }

    public async Task<ResultNone> SetAcceptPatreonEmailAsync(long userId, bool accept = true) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
Update "UserInfo"
Set "PatronEmailAccept" = {accept}
WHERE "UserId" = {userId}
""";

        return (await db.SafeQueryAsync(sql))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> SetPatreonEmailAsync(long userId, string email) {
        var db = await _dbContext.GetConnectionAsync();
        var sql = $"""
Update "UserInfo" 
Set "PatronEmail" = @PatronEmail, "PatronEmailAccept" = false
WHERE "UserId" = {userId}
""";
        return (await db.SafeQueryAsync(sql, new { PatronEmail = email }))
            .LogIfError(Logger)
            .ToNone();
    }

    public async Task<ResultNone> SetActiveAsync(long userId, bool isActive) {
        var db = await _dbContext.GetConnectionAsync();
        const string sql = """
                   Update "UserInfo" 
                   Set "Active" = @Active
                   WHERE "UserId" = @UserId
                   """;
        return (await db.SafeQueryAsync(sql, new { Active = isActive }))
               .LogIfError(Logger)
               .ToNone();
    }
}