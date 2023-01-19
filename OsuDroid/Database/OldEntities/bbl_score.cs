using NPoco;

namespace OsuDroid.Database.OldEntities;

internal sealed class bbl_score {
    [Column("id")] public long Id { get; set; }
    [Column("uid")] public long Uid { get; set; }
    [Column("filename")] public string? Filename { get; set; }
    [Column("hash")] public string? Hash { get; set; }
    [Column("mode")] public string? Mode { get; set; }
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