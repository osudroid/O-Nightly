using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_global_ranking_timeline")]
[ExplicitColumns]
[PrimaryKey(new[] { "UserId", "Date" }, AutoIncrement = false)]
public class BblGlobalRankingTimeline {
    [Column("user_id")] public long UserId { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("global_ranking")] public long GlobalRanking { get; set; }
    [Column("score")] public long Score { get; set; }
}