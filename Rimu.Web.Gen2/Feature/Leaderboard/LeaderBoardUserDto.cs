using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Web.Gen2.Feature.Leaderboard;

public class LeaderBoardUserDto {
    public long RankNumber { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Region { get; set; }
    public long OverallScore { get; set; }
    public long OverallPlaycount { get; set; }
    public long OverallSs { get; set; }
    public long OverallS { get; set; }
    public long OverallA { get; set; }
    public long OverallAccuracy { get; set; }

    public static LeaderBoardUserDto FromLeaderBoardUser(LeaderBoardUser leaderBoardUser) {
        return new LeaderBoardUserDto {
            RankNumber = leaderBoardUser.RankNumber,
            UserId = leaderBoardUser.UserId,
            Username = leaderBoardUser.Username,
            Region = leaderBoardUser.Region,
            OverallScore = leaderBoardUser.OverallScore,
            OverallPlaycount = leaderBoardUser.OverallPlaycount,
            OverallSs = leaderBoardUser.OverallSs,
            OverallS = leaderBoardUser.OverallS,
            OverallA = leaderBoardUser.OverallA,
            OverallAccuracy = leaderBoardUser.OverallAccuracy
        };
    }
}