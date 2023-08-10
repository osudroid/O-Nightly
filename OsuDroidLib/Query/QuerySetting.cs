using Npgsql;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query;

public static class QuerySetting {
    public static async Task<Result<Option<Database.Entities.Setting>, string>> GetSetting(
        NpgsqlConnection db,
        string mainKey,
        string subKey) {
        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
AND SubKey = @SubKey
";
        return await db.SafeQueryFirstOrDefaultAsync<Database.Entities.Setting>(
            sql,
            new { MainKey = mainKey, SubKey = subKey }
        );
    }

    public static async Task<Result<IEnumerable<Database.Entities.Setting>, string>> GetSettingsAsync(
        NpgsqlConnection db,
        string mainKey) {
        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
";

        return await db.SafeQueryAsync<Database.Entities.Setting>(sql, new { MainKey = mainKey });
    }

    public static async Task<Result<IEnumerable<Database.Entities.Setting>, string>> GetAllAsync(
        NpgsqlConnection db) {
        var sql = @"SELECT * FROM Setting";

        return await db.SafeQueryAsync<Database.Entities.Setting>(sql);
    }
}