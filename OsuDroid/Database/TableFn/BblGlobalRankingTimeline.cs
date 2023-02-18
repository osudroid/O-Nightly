using NPoco;

namespace OsuDroid.Database.TableFn;

[TableName("bbl_global_ranking_timeline")]
[ExplicitColumns]
[PrimaryKey(new[] { "UserId", "Date" }, AutoIncrement = false)]
public static class BblGlobalRankingTimeline {
    public static Entities.BblGlobalRankingTimeline FromLeaderBoardUser(Entities.LeaderBoardUser leaderBoardUser,
        DateTime dateTime) {
        return new Entities.BblGlobalRankingTimeline {
            UserId = leaderBoardUser.Id,
            Date = dateTime,
            Score = leaderBoardUser.OverallScore,
            GlobalRanking = leaderBoardUser.Rank
        };
    }

    public static Result<Option<Entities.BblGlobalRankingTimeline>, string> Now(SavePoco db, long userId) {
        var resultLeaderBoardUser = LeaderBoardUser.LeaderBoardUserSingleUser(db, userId);

        if (resultLeaderBoardUser == EResult.Err) {
            return Result<Option<Entities.BblGlobalRankingTimeline>, string>.Err(resultLeaderBoardUser.Err());
        }

        var leaderBoardUser = resultLeaderBoardUser.Ok();

        if (leaderBoardUser.IsSet()) {
            return Result<Option<Entities.BblGlobalRankingTimeline>, string>
                .Ok(Option<Entities.BblGlobalRankingTimeline>
                    .With(FromLeaderBoardUser(leaderBoardUser.Unwrap(), DateTime.UtcNow)));
        }
        return Result<Option<Entities.BblGlobalRankingTimeline>, string>
            .Ok(Option<Entities.BblGlobalRankingTimeline>.Empty);
    }

    public static Result<IReadOnlyList<Entities.BblGlobalRankingTimeline>, string> BuildTimeLine(SavePoco db, long userId,
        DateTime startAt) {
        var sql = new Sql(@$"
SELECT * 
FROM bbl_global_ranking_timeline
WHERE user_id = {userId}
AND date >= @0
ORDER BY date ASC 
", startAt);

        var resultFetch = db.Fetch<Entities.BblGlobalRankingTimeline>(sql);
        if (resultFetch == EResult.Err)
            return Result<IReadOnlyList<Entities.BblGlobalRankingTimeline>, string>.Err(resultFetch.Err());
        var rankingTimelines = resultFetch.Ok();
        var resList = rankingTimelines.Count == 0
            ? new List<Entities.BblGlobalRankingTimeline>(0)
            : rankingTimelines;
        
        var resultNow = Now(db, userId);
        if (resultNow == EResult.Err)
            return Result<IReadOnlyList<Entities.BblGlobalRankingTimeline>, string>.Err(resultNow.Err());
        var optionNow = resultNow.Ok();
        if (optionNow.IsSet())
            resList.Add(optionNow.Unwrap());
        return Result<IReadOnlyList<Entities.BblGlobalRankingTimeline>, string>.Ok(resList);
    }
    
    public static ResultErr<string> DeleteAllRankingByAllUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_global_ranking_timeline WHERE user_id = {userId}");
    }
}