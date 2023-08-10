using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("GlobalRankingTimeline")]
public class GlobalRankingTimeline {
    [ExplicitKey] public long UserId { get; set; }
    [ExplicitKey] public DateTime Date { get; set; }
    public long GlobalRanking { get; set; }
    public long Score { get; set; }

    public static GlobalRankingTimeline FromLeaderBoardUser(
        LeaderBoardUser leaderBoardUser,
        DateTime dateTime) {
        return new GlobalRankingTimeline {
            UserId = leaderBoardUser.UserId,
            Date = dateTime,
            Score = leaderBoardUser.OverallScore,
            GlobalRanking = leaderBoardUser.RankNumber
        };
    }
}