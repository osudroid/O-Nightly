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


    public sealed class StatisticActiveUser {
        public long ActiveUserLast1H { get; set; }
        public long ActiveUserLast1Day { get; set; }
        public long RegisterUser { get; set; }
    }
}