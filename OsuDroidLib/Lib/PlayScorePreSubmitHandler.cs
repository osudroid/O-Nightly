using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Lib;

public static class PlayScorePreSubmitHandler {
    public static async Task<Result<Option<PlayScorePreSubmit>, string>> GetByIdAsync(NpgsqlConnection db, long id) {
        return await QueryPlayScorePreSubmit.GetByIdAsync(db, id);
    }
}