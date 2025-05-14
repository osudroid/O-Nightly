using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class UserStats: IUserStatsReadonly {
    public required long UserId { get; set; }
    public required long OverallPlaycount { get; set; }
    public required long OverallScore { get; set; }
    public required double OverallAccuracy { get; set; }
    public required long OverallCombo { get; set; }
    public required long OverallXss { get; set; }
    public required long OverallSs { get; set; }
    public required long OverallXs { get; set; }
    public required long OverallS { get; set; }
    public required long OverallA { get; set; }
    public required long OverallB { get; set; }
    public required long OverallC { get; set; }
    public required long OverallD { get; set; }
    public required long OverallPerfect { get; set; }
    public required long OverallHits { get; set; }
    public required long Overall300 { get; set; }
    public required long Overall100 { get; set; }
    public required long Overall50 { get; set; }
    public required long OverallGeki { get; set; }
    public required long OverallKatu { get; set; }
    public required long OverallMiss { get; set; }
    public required double OverallPp { get; set; }


    /// <summary> Need OverallAccuracy And Playcount </summary>
    /// <returns></returns>
    public double FormatAccuracy() {
        return FormatAccuracy(OverallPlaycount, OverallAccuracy);
    }

    public static double FormatAccuracy(double playcount, double overallAccuracy) {
        return playcount == 0 ? 100000 : overallAccuracy / playcount;
    }
}