// ReSharper disable All

namespace OsuDroidLib.Database.Entities;

public class LeaderBoardUser {
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
}