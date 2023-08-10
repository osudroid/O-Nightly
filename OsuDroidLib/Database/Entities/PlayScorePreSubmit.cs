using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("PlayScorePreSubmit")]
public class PlayScorePreSubmit {
    [ExplicitKey] public long PlayScoreId { get; set; }
    public long UserId { get; set; }
    public string? Filename { get; set; }
    public string? Hash { get; set; }
    public string[]? Mode { get; set; }
    public long Score { get; set; }
    public long Combo { get; set; }
    public string? Mark { get; set; }
    public long Geki { get; set; }
    public long Perfect { get; set; }
    public long Katu { get; set; }
    public long Good { get; set; }
    public long Bad { get; set; }
    public long Miss { get; set; }
    public DateTime Date { get; set; }
    public long Accuracy { get; set; }
}