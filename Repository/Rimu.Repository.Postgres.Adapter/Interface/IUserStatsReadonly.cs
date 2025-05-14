namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IUserStatsReadonly {
    public long UserId { get; }
    public long OverallPlaycount { get; }
    public long OverallScore { get; }
    public double OverallAccuracy { get; }
    public double OverallPp { get; }
    public long OverallCombo { get; }
    public long OverallXss { get; }
    public long OverallSs { get; }
    public long OverallXs { get; }
    public long OverallS { get; }
    public long OverallA { get; }
    public long OverallB { get; }
    public long OverallC { get; }
    public long OverallD { get; }
    public long OverallPerfect { get; }
    public long OverallHits { get; }
    public long Overall300 { get; }
    public long Overall100 { get; }
    public long Overall50 { get; }
    public long OverallGeki { get; }
    public long OverallKatu { get; }
    public long OverallMiss { get; }
}