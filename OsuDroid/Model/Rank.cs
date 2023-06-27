using Npgsql;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class Rank {
    public static async Task<Result<IReadOnlyList<QueryPlayScore.MapTopPlays>, string>>
        MapTopPlaysByFilenameAndHashAsync(
            NpgsqlConnection db, string filename, string fileHash, int maxResult) {
        return await QueryPlayScore.MapTopPlaysByFilenameAndHashAsync(db, filename, fileHash, maxResult);
    }
}