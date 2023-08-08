namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewProfileStats : IView {
    public long Id { get; set; }
    public string? Username { get; set; }
    public bool Found { get; set; }
    public string? Region { get; set; }
    public bool Active { get; set; }
    public bool Supporter { get; set; }
    public DateTime RegistTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    public long GlobalRanking { get; set; }
    public long CountryRanking { get; set; }

    public long OverallScore { get; set; }
    public long OverallAccuracy { get; set; }
    public long OverallCombo { get; set; }
    public long OverallXss { get; set; }
    public long OverallSs { get; set; }
    public long OverallXs { get; set; }
    public long OverallS { get; set; }
    public long OverallA { get; set; }
    public long OverallB { get; set; }
    public long OverallC { get; set; }
    public long OverallD { get; set; }
    public long OverallHits { get; set; }
    public long OverallPerfect { get; set; }
    public long Overall300 { get; set; }
    public long Overall100 { get; set; }
    public long Overall50 { get; set; }
    public long OverallGeki { get; set; }
    public long OverallKatu { get; set; }
    public long OverallMiss { get; set; }
    public long OverallPlaycount { get; set; }
}