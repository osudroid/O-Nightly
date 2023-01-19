using NPoco;

namespace OsuDroidLib.Database.Entities;
#nullable enable
[TableName("bbl_score_banned")]
[ExplicitColumns]
[PrimaryKey(new[] { "id" }, AutoIncrement = true)]
public class BblScoreBanned {
    [Column("id")] public long Id { get; set; }
    [Column("uid")] public long Uid { get; set; }

    /// <summary> varchar(255) </summary>
    [Column("filename")]
    public string? Filename { get; set; }

    /// <summary> varchar(36) </summary>
    [Column("hash")]
    public string? Hash { get; set; }

    /// <summary> varchar(8) </summary>
    [Column("mode")]
    public string? Mode { get; set; }

    [Column("score")] public long Score { get; set; }
    [Column("combo")] public long Combo { get; set; }
    [Column("mark")] public string? Mark { get; set; }
    [Column("geki")] public long Geki { get; set; }
    [Column("perfect")] public long Perfect { get; set; }
    [Column("katu")] public long Katu { get; set; }
    [Column("good")] public long Good { get; set; }
    [Column("bad")] public long Bad { get; set; }
    [Column("miss")] public long Miss { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("accuracy")] public long Accuracy { get; set; }
}