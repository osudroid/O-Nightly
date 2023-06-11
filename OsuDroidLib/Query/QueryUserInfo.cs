using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryUserInfo {
    public static async Task<ResultErr<string>> InsertAsync(NpgsqlConnection db, UserInfo userInfo) {
        return await db.SafeInsertAsync(userInfo);
    }

    public static async Task<Result<Option<UserInfo>, string>> GetUserIdByUsernameAsync(NpgsqlConnection db, string username) {
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(
            "SELECT UserId FROM UserInfo WHERE Username = lower(@Username) LIMIT 1",
            new { Username = username });
    }
    
    public static async Task<Result<Option<StatisticActiveUser>, string>> GetStatisticActiveUser(NpgsqlConnection db) {
        var sql = @"
SELECT 
    count(*) as register_user,
    sum(CASE WHEN userinfo.LastLoginTime >= @LLT0 THEN 1 ELSE 0 END) as ActiveUserLast1H,
    sum(CASE WHEN userinfo.LastLoginTime >= @LLT1 THEN 1 ELSE 0 END) as ActiveUserLast1Day
FROM UserInfo 
WHERE banned = false 
";
        return await db.SafeQueryFirstOrDefaultAsync<StatisticActiveUser>(
            sql,
            new { LLT0 = DateTime.UtcNow - TimeSpan.FromHours(1), LLT1 = DateTime.UtcNow - TimeSpan.FromDays(1)}
            );        
    }

    public static async Task<Result<Option<UserInfo>, string>> GetByUserIdAsync(NpgsqlConnection db, long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@$"
SELECT * 
FROM UserInfo
WHERE USerId = {userId}
");
    }
    
    public static async Task<Result<Option<UserInfo>, string>> GetByUsernameAsync(NpgsqlConnection db, string username) {
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@$"
SELECT * 
FROM UserInfo
WHERE Username = @Username
", new { Username = username });
    }
    
    public static async Task<Result<Option<UserInfo>, string>> GetByEmailAsync(NpgsqlConnection db, string email) {
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@$"
SELECT * 
FROM UserInfo
WHERE Email = lower(@Email)
", new { Email = email });
    }
    
    public static async Task<Result<Option<UserInfo>, string>> GetUsernameAndRegionByUserId(
        NpgsqlConnection db, long userId) {

        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@$"
SELECT username, region
FROM UserInfo
WHERE UserId = {userId}
");
    }
    
    public static async Task<Result<Option<UserInfo>, string>> GetUsernameByUserIdAsync(
        NpgsqlConnection db, long userId) {

        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@$"
SELECT Username
FROM UserInfo
WHERE UserId = {userId}
");
    }

    public static async Task<ResultErr<string>> DeleteAsync(NpgsqlConnection db, long userId) {
        return await db.SafeQueryAsync($"DELETE FROM UserInfo WHERE UserId = {userId}");
    }

    public static async Task<Result<Option<UserInfo>, string>> GetIdUsernamePasswordByLowerUsernameAsync(
        NpgsqlConnection db, string username) {

        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@"
SELECT UserId, Username, Password 
FROM UserInfo 
WHERE lower(username) = lower(@Username)", 
            new { Username = username });
    }

    public static async Task<Result<Option<UserInfo>, string>> GetLoginInfoByEmailAndPasswordByEmailAndPasswordAsync(
        NpgsqlConnection db, string email, string passwordHash) {
        
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@"
SELECT UserId, Email, Password, Username 
FROM UserInfo 
WHERE Email = lower(@Email) 
  AND Banned = false 
  AND Password = @Password 
LIMIT 1", 
            new { Email = email, Password = passwordHash });
    }
    
    public static async Task<Result<Option<UserInfo>, string>> GetLoginInfoByEmailAndPasswordByUsernameAndPasswordAsync(
        NpgsqlConnection db, string username, string passwordHash) {
        
        return await db.SafeQueryFirstOrDefaultAsync<UserInfo>(@"
SELECT UserId, Email, Password, Username
FROM UserInfo 
WHERE Username = lower(@Username) 
  AND Banned = false 
  AND Password = @Password 
LIMIT 1", 
            new { Username = username, Password = passwordHash });
    }
    
    public static async Task<ResultErr<string>> UpdateIpAndRegionAsync(
        NpgsqlConnection db, long userId, string region, string latestIp) {
        
        return await db.SafeQueryAsync(@"
UPDATE UserInfo
SET Region = @Region, LatestIp = @LatestIp
WHERE UserId = @UserId 
", new { Region = region, LatestIp = latestIp, UserId = userId });
    }

    public static async Task<ResultErr<string>> UpdateEmailByUserId(
        NpgsqlConnection db, long userId, string email) {

        return await db.SafeQueryAsync(
            @$"UPDATE UserInfo SET Email = lower(@Email) WHERE UserId = {userId}",
            new { Email = email }
            );
    }
    
    public static async Task<ResultErr<string>> UpdatePasswordByUserIdAsync(
        NpgsqlConnection db, long userId, string password) {

        return await db.SafeQueryAsync(
            @$"UPDATE UserInfo SET Password = lower(@Password) WHERE UserId = {userId}",
            new { Password = password }
        );
    }
    
    public static async Task<ResultErr<string>> UpdateUsernameByUserIdAsync(
        NpgsqlConnection db, long userId, string username) {

        return await db.SafeQueryAsync(
            @$"UPDATE UserInfo SET Username = lower(@Username) WHERE UserId = {userId}",
            new { Username = username }
        );
    }
    
    public static async Task<Result<Option<UserInfo>, string>> CheckPasswordGetIdAndUsernameAsync(
        NpgsqlConnection db, string passwordHash) {

        var sql = @"
SELECT UserId, Username
FROM UserInfo
WHERE password = @PasswordHash
";
        return (await db.SafeQueryFirstOrDefaultAsync<UserInfo>(sql,
                new { PasswordHash = passwordHash }));
    }
    
    public static async Task<Result<IEnumerable<UserInfo>, string>> GetEmailAndUsernameByEmailAndUsername(
        NpgsqlConnection db, string email, string username) {

        var sql = @"
SELECT username, email 
FROM UserInfo
WHERE Email = lower(@Email)
   Or Username = lower(@Username)
";

        return await db.SafeQueryAsync<UserInfo>(sql, new { Username = username, Email = email });
    }
    
    public static async Task<Result<bool, string>> CheckExistByEmailAndUsername(
        NpgsqlConnection db, string email, string username) {

        return (await GetEmailAndUsernameByEmailAndUsername(db, email, username)).Map(x => x.Any());
    }

    public static async Task<ResultErr<string>> UpdatePasswordAsync(
        NpgsqlConnection db, long userId, string passwordHash) {

        var sql = @"
UPDATE UserInfo
SET Password = @Password
WHERE UserId = @UserId 
";
        return await db.SafeQueryAsync(
            sql,
            new  { Password = passwordHash, UserId = userId }
            );
    }
    
    public static async Task<Result<DateTime, string>> UpdateLastLoginTimeAsync(NpgsqlConnection db, long userId) {
        var time = DateTime.UtcNow;

        return (await db.SafeQueryAsync(
            $"UPDATE UserInfo SET LastLoginTime = @Time WHERE UserId = {userId}", 
            new { Time  = time }
        )).Map(x => time);
    }
    
    public static async Task<ResultErr<string>> SetAcceptPatreonEmailAsync(NpgsqlConnection db, long userId, bool accept = true) {
        var sql = $@"
Update UserInfo 
Set PatronEmailAccept = {accept}
WHERE UserId = {userId}
";

        return await db.SafeQueryAsync(sql);
    }

    public static async Task<ResultErr<string>> SetPatreonEmailAsync(NpgsqlConnection db, long userId, string email) {
        var sql = $@"
Update UserInfo 
Set PatronEmail = @PatronEmail, PatronEmailAccept = false
WHERE UserId = {userId}
";
        return await db.SafeQueryAsync(sql, new { PatronEmail = email });
    }
    
    public sealed class StatisticActiveUser {
        public long ActiveUserLast1H { get; set; }
        public long ActiveUserLast1Day { get; set; }
        public long RegisterUser { get; set; }
    }
}