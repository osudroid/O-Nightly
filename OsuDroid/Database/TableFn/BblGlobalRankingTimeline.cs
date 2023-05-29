using NPoco;

namespace OsuDroid.Database.TableFn;

[TableName("bbl_global_ranking_timeline")]
[ExplicitColumns]
[PrimaryKey(new[] { "UserId", "Date" }, AutoIncrement = false)]
public static class BblGlobalRankingTimeline {
    public static Entities.GlobalRankingTimeline FromLeaderBoardUser(Entities.LeaderBoardUser leaderBoardUser,
        DateTime dateTime) {
        return new Entities.GlobalRankingTimeline {
            UserId = leaderBoardUser.UserId,
            Date = dateTime,
            Score = leaderBoardUser.OverallScore,
            GlobalRanking = leaderBoardUser.RankNumber
        };
    }

    public static Result<Option<Entities.GlobalRankingTimeline>, string> Now(SavePoco db, long userId) {
        var resultLeaderBoardUser = LeaderBoardUser.LeaderBoardUserSingleUser(db, userId);

        if (resultLeaderBoardUser == EResult.Err) {
            return Result<Option<Entities.GlobalRankingTimeline>, string>.Err(resultLeaderBoardUser.Err());
        }

        var leaderBoardUser = resultLeaderBoardUser.Ok();

        if (leaderBoardUser.IsSet()) {
            return Result<Option<Entities.GlobalRankingTimeline>, string>
                .Ok(Option<Entities.GlobalRankingTimeline>
                    .With(FromLeaderBoardUser(leaderBoardUser.Unwrap(), DateTime.UtcNow)));
        }
        return Result<Option<Entities.GlobalRankingTimeline>, string>
            .Ok(Option<Entities.GlobalRankingTimeline>.Empty);
    }

    public static Result<IReadOnlyList<Entities.GlobalRankingTimeline>, string> BuildTimeLine(SavePoco db, long userId,
        DateTime startAt) {
        var sql = new Sql(@$"
SELECT * 
FROM bbl_global_ranking_timeline
WHERE user_id = {userId}
AND date >= @0
ORDER BY date ASC 
", startAt);

        var resultFetch = db.Fetch<Entities.GlobalRankingTimeline>(sql);
        if (resultFetch == EResult.Err)
            return Result<IReadOnlyList<Entities.GlobalRankingTimeline>, string>.Err(resultFetch.Err());
        var rankingTimelines = resultFetch.Ok();
        var resList = rankingTimelines.Count == 0
            ? new List<Entities.GlobalRankingTimeline>(0)
            : rankingTimelines;
        
        var resultNow = Now(db, userId);
        if (resultNow == EResult.Err)
            return Result<IReadOnlyList<Entities.GlobalRankingTimeline>, string>.Err(resultNow.Err());
        var optionNow = resultNow.Ok();
        if (optionNow.IsSet())
            resList.Add(optionNow.Unwrap());
        return Result<IReadOnlyList<Entities.GlobalRankingTimeline>, string>.Ok(resList);
    }
    
    public static ResultErr<string> DeleteAllRankingByAllUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_global_ranking_timeline WHERE user_id = {userId}");
    }
}