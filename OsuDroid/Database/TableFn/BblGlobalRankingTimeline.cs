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

    public static Entities.BblGlobalRankingTimeline? Now(SavePoco db, long userId) {
        var leaderBoardUser = LeaderBoardUser.LeaderBoardUserSingleUser(db, userId);

        return leaderBoardUser is null
            ? null
            : FromLeaderBoardUser(leaderBoardUser, DateTime.UtcNow);
    }

    public static IReadOnlyList<Entities.BblGlobalRankingTimeline> BuildTimeLine(SavePoco db, long userId,
        DateTime startAt) {
        var sql = new Sql(@$"
SELECT * 
FROM bbl_global_ranking_timeline
WHERE user_id = {userId}
AND date >= @0
ORDER BY date ASC 
", startAt);

        var response = db.Fetch<Entities.BblGlobalRankingTimeline>(sql);
        var resList = response == EResponse.Err
            ? new List<Entities.BblGlobalRankingTimeline>(0)
            : response.Ok();
        // if (now is not null)
        //     resList.Add(now);
        return resList;
    }
    
    public static Response DeleteAllRankingByAllUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_global_ranking_timeline WHERE user_id = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}