using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;
using Db = Npgsql.NpgsqlConnection;

namespace OsuDroidLib.Manager;

public static class WebLoginMathResultManager {
    public static async ValueTask<ResultErr<string>>
        AddWebLoginTokenAsync(Db db, WebLoginMathResult webLoginMathResult) {
        return await QueryWebLoginMathResult.AddWebLoginTokenAsync(db, webLoginMathResult);
    }

    public static async ValueTask<ResultErr<string>> DeleteWebLoginTokenAsyncByTokenIdAndUserId(Db db, Guid id) {
        return await QueryWebLoginMathResult.DeleteWebLoginTokenAsync(db, id);
    }

    public static async ValueTask<Result<Option<WebLoginMathResult>, string>> GetWebLoginTokenAsync(Db db, Guid token) {
        return await QueryWebLoginMathResult.GetWebLoginTokenAsync(db, token);
    }

    public static async ValueTask<ResultErr<string>> DeleteOldTokens(Db db, TimeSpan timeSpan) {
        return await QueryWebLoginMathResult.DeleteOldTokens(db, timeSpan);
    }
}