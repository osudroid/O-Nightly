using NPoco;

namespace OsuDroid.Database.OldEntities;

internal sealed class bbl_score {
    [Column("id")] public long id { get; set; }
    [Column("uid")] public long uid { get; set; }
    [Column("filename")] public string? filename { get; set; }
    [Column("hash")] public string? hash { get; set; }
    [Column("mode")] public string? mode { get; set; }
    [Column("score")] public long score { get; set; }
    [Column("combo")] public long combo { get; set; }
    [Column("mark")] public string? mark { get; set; }
    [Column("geki")] public long geki { get; set; }
    [Column("perfect")] public long perfect { get; set; }
    [Column("katu")] public long katu { get; set; }
    [Column("good")] public long good { get; set; }
    [Column("bad")] public long bad { get; set; }
    [Column("miss")] public long miss { get; set; }
    [Column("date")] public DateTime date { get; set; }
    [Column("accuracy")] public long accuracy { get; set; }
}