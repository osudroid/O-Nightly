namespace Rimu.Terminal.ImportCSVFile;

internal struct bbl_score {
    public required long id { get; set; }
    public required long uid { get; set; }
    public required string? filename { get; set; }
    public required string? hash { get; set; }
    public required string? mode { get; set; }
    public required long score { get; set; }
    public required long combo { get; set; }
    public required string? mark { get; set; }
    public required long geki { get; set; }
    public required long perfect { get; set; }
    public required long katu { get; set; }
    public required long good { get; set; }
    public required long bad { get; set; }
    public required long miss { get; set; }
    public required DateTime date { get; set; }
    public required double accuracy { get; set; }
    public required double? pp { get; set; }
}