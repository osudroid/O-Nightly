using NPoco;

namespace OsuDroidLib.Database.Entities;

public class LeaderBoardUser {
    [Column("rank_number")] public long Rank { get; set; }
    [Column("id")] public long Id { get; set; }
    [Column("username")] public string? Username { get; set; }
    [Column("region")] public string? Region { get; set; }
    [Column("overall_score")] public long OverallScore { get; set; }
    [Column("playcount")] public long Playcount { get; set; }
    [Column("overall_ss")] public long OverallSs { get; set; }
    [Column("overall_s")] public long OverallS { get; set; }
    [Column("overall_a")] public long OverallA { get; set; }
    [Column("overall_accuracy")] public long OverallAccuracy { get; set; }
}