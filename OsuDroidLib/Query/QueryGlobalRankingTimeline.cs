using System.Runtime.InteropServices;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;

namespace OsuDroidLib.Query;

public static class QueryGlobalRankingTimeline {
    public static async Task<Result<Option<GlobalRankingTimeline>, string>> Now(NpgsqlConnection db, long userId) {
        var resultLeaderBoardUser = await Query.LeaderBoardUserSingleUser(db, userId);

        if (resultLeaderBoardUser == EResult.Err) {
            return resultLeaderBoardUser.ChangeOkType<Option<GlobalRankingTimeline>>();
        }

        var leaderBoardUser = resultLeaderBoardUser.Ok();

        if (leaderBoardUser.IsSet()) {
            return Result<Option<GlobalRankingTimeline>, string>
                .Ok(Option<GlobalRankingTimeline>
                    .With(GlobalRankingTimeline.FromLeaderBoardUser(leaderBoardUser.Unwrap(), DateTime.UtcNow)));
        }

        return Result<Option<GlobalRankingTimeline>, string>
            .Ok(Option<GlobalRankingTimeline>.Empty);
    }

    public static async Task<Result<IReadOnlyList<GlobalRankingTimeline>, string>>
        BuildTimeLineAsync(NpgsqlConnection db, long userId, DateTime startAt) {
        var sql = @$"
SELECT * 
FROM GlobalRankingTimeline
WHERE UserId = {userId}
AND date >= @StartAt
ORDER BY date ASC 
";

        var resultFetch = await db.SafeQueryAsync<GlobalRankingTimeline>(sql, new { StartAt = startAt });
        if (resultFetch == EResult.Err)
            return Result<IReadOnlyList<GlobalRankingTimeline>, string>.Err(resultFetch.Err());
        var rankingTimelines = resultFetch.Ok().ToList();
        var resList = rankingTimelines.Count == 0
            ? new List<GlobalRankingTimeline>(0)
            : rankingTimelines;

        var resultNow = await Now(db, userId);
        if (resultNow == EResult.Err)
            return Result<IReadOnlyList<GlobalRankingTimeline>, string>.Err(resultNow.Err());
        var optionNow = resultNow.Ok();
        if (optionNow.IsSet())
            resList.Add(optionNow.Unwrap());
        return Result<IReadOnlyList<GlobalRankingTimeline>, string>.Ok(resList);
    }

    public static async Task<ResultErr<string>> DeleteAllRankingByAllUserId(NpgsqlConnection db, long userId) {
        return await db.SafeQueryAsync(@$"Delete FROM GlobalRankingTimeline WHERE UserId = {userId}");
    }
}