using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryUserInfo {
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
    
    public sealed class StatisticActiveUser {
        public long ActiveUserLast1H { get; set; }
        public long ActiveUserLast1Day { get; set; }
        public long RegisterUser { get; set; }
    }
}