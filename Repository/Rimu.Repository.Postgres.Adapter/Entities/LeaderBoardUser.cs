// ReSharper disable All

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class LeaderBoardUser {
    public required long RankNumber { get; set; }
    public required long UserId { get; set; }
    public required string? Username { get; set; }
    public required string? Region { get; set; }
    public required long OverallScore { get; set; }
    public required long OverallPlaycount { get; set; }
    public required long OverallSs { get; set; }
    public required long OverallS { get; set; }
    public required long OverallA { get; set; }
    public required long OverallAccuracy { get; set; }
    public required long OverallPp { get; set; }

    public LeaderBoardUser() {
    }
}