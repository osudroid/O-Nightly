using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QuerySetting {
    public static async Task<Result<Option<Setting>, string>> GetSetting(
        NpgsqlConnection db, string mainKey, string subKey) {

        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
AND SubKey = @SubKey
";
        return await db.SafeQueryFirstOrDefaultAsync<Setting>(
            sql,
            new { MainKey = mainKey, SubKey = subKey }
            );
    }
    
    public static async Task<Result<IEnumerable<Setting>, string>> GetSettingsAsync(
        NpgsqlConnection db, string mainKey) {

        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
";

        return await db.SafeQueryAsync<Setting>(sql, new { MainKey = mainKey });
    }
}