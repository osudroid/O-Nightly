namespace Rimu.Repository.Postgres.Adapter.Entities;

[System.ComponentModel.DataAnnotations.Schema.Table("GlobalRankingTimeline")]
public class GlobalRankingTimeline {
    public required long UserId { get; set; }
    public required DateTime Date { get; set; }
    public required long GlobalRanking { get; set; }
    public required double Pp { get; set; }

    public GlobalRankingTimeline() {
    }

    public static GlobalRankingTimeline FromLeaderBoardUser(
        LeaderBoardUser leaderBoardUser,
        DateTime dateTime) {
        return new GlobalRankingTimeline {
            UserId = leaderBoardUser.UserId,
            Date = dateTime,
            Pp = leaderBoardUser.OverallPp,
            GlobalRanking = leaderBoardUser.RankNumber
        };
    }
}