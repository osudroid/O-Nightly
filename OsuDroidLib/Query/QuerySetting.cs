using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query;

public static class QuerySetting {
    public static async Task<Result<Option<OsuDroidLib.Database.Entities.Setting>, string>> GetSetting(
        NpgsqlConnection db, string mainKey, string subKey) {
        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
AND SubKey = @SubKey
";
        return await db.SafeQueryFirstOrDefaultAsync<OsuDroidLib.Database.Entities.Setting>(
            sql,
            new { MainKey = mainKey, SubKey = subKey }
        );
    }

    public static async Task<Result<IEnumerable<OsuDroidLib.Database.Entities.Setting>, string>> GetSettingsAsync(
        NpgsqlConnection db, string mainKey) {
        var sql = @"
SELECT * 
FROM Setting
WHERE MainKey = @MainKey
";

        return await db.SafeQueryAsync<OsuDroidLib.Database.Entities.Setting>(sql, new { MainKey = mainKey });
    }

    public static async Task<Result<IEnumerable<OsuDroidLib.Database.Entities.Setting>, string>> GetAllAsync(
        NpgsqlConnection db) {
        var sql = @"SELECT * FROM Setting";

        return await db.SafeQueryAsync<OsuDroidLib.Database.Entities.Setting>(sql);
    }
}