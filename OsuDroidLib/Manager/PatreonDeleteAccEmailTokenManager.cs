using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class PatreonDeleteAccEmailTokenManager {
    public static async ValueTask<ResultErr<string>>
        InsertAsync(NpgsqlConnection db, PatreonDeleteAccEmailToken value) {
        return await QueryPatreonDeleteAccEmailToken.InsertAsync(db, value);
    }

    public static async ValueTask<ResultErr<string>> RemoveToOldRows(NpgsqlConnection db) {
        var limit = DateTime.UtcNow - TimeSpan.FromMinutes(20);
        return await QueryPatreonDeleteAccEmailToken.RemoveToOldRows(db, limit);
    }

    public static async ValueTask<Result<Option<PatreonDeleteAccEmailToken>, string>> FindByTokenWithLimitTime(
        NpgsqlConnection db,
        Guid token) {
        var limit = DateTime.UtcNow - TimeSpan.FromMinutes(20);

        return await QueryPatreonDeleteAccEmailToken.FindByTokenWithLimitTime(db, token, limit);
    }
}