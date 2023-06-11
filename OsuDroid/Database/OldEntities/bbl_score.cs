namespace OsuDroid.Database.OldEntities;

internal sealed class bbl_score {
    public long id { get; set; }
    public long uid { get; set; }
    public string? filename { get; set; }
    public string? hash { get; set; }
    public string? mode { get; set; }
    public long score { get; set; }
    public long combo { get; set; }
    public string? mark { get; set; }
    public long geki { get; set; }
    public long perfect { get; set; }
    public long katu { get; set; }
    public long good { get; set; }
    public long bad { get; set; }
    public long miss { get; set; }
    public DateTime date { get; set; }
    public long accuracy { get; set; }
}