namespace OsuDroid.Database.OldEntities;

public sealed class bbl_score_banned {
    public long Id { get; set; }
    public long Uid { get; set; }
    public string? Filename { get; set; }
    public string? Hash { get; set; }
    public string? Mode { get; set; }
    public long Score { get; set; }
    public long Combo { get; set; }
    public long Mark { get; set; }
    public long Geki { get; set; }
    public long Perfect { get; set; }
    public long Katu { get; set; }
    public long Good { get; set; }
    public long Bad { get; set; }
    public long Miss { get; set; }
    public DateTime Date { get; set; }
    public long Accuracy { get; set; }
}