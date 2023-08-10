using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class PatreonEmailTokenManager {
    public static async ValueTask<ResultErr<string>> InsertAsync(NpgsqlConnection db, PatreonEmailToken value) {
        return await QueryPatreonEmailToken.InsertAsync(db, value);
    }

    public static async ValueTask<ResultErr<string>> RemoveToOldRows(NpgsqlConnection db) {
        var limit = DateTime.UtcNow - TimeSpan.FromMinutes(20);
        return await QueryPatreonEmailToken.RemoveToOldRows(db, limit);
    }

    public static async ValueTask<Result<Option<PatreonEmailToken>, string>> FindByTokenWithLimitTimeAsync(
        NpgsqlConnection db,
        Guid token) {
        var limit = DateTime.UtcNow - TimeSpan.FromMinutes(20);

        return await QueryPatreonEmailToken.FindByTokenWithLimitTime(db, token, limit);
    }
}