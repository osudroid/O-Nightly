using Npgsql;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class PlayRecent {
    /// <summary>
    /// </summary>
    /// <param name="filterPlays"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="startAt"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<Result<List<Entities.PlayScoreWithUsername>, string>> FilterByAsync(
        NpgsqlConnection db,
        string filterPlays,
        string orderBy,
        int limit,
        int startAt) {
        return (await Query.PlayRecentFilterByAsync(
            db,
            filterPlays,
            orderBy,
            limit,
            startAt
        )).Map(x => x.ToList());
    }
}