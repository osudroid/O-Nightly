namespace OsuDroid.View;

// ReSharper disable All
public class ViewUserStats : IView {
    public long UserId { get; set; }
    public long OverallPlaycount { get; set; }
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
    public long OverallPerfect { get; set; }
    public long OverallHits { get; set; }
    public long Overall300 { get; set; }
    public long Overall100 { get; set; }
    public long Overall50 { get; set; }
    public long OverallGeki { get; set; }
    public long OverallKatu { get; set; }
    public long OverallMiss { get; set; }

    public static ViewUserStats FromUserStats(Entities.UserStats userStats) {
        return new ViewUserStats {
            UserId = userStats.UserId,
            OverallPlaycount = userStats.OverallPlaycount,
            OverallScore = userStats.OverallScore,
            OverallAccuracy = userStats.OverallAccuracy,
            OverallCombo = userStats.OverallCombo,
            OverallXss = userStats.OverallXss,
            OverallSs = userStats.OverallSs,
            OverallXs = userStats.OverallXs,
            OverallS = userStats.OverallS,
            OverallA = userStats.OverallA,
            OverallB = userStats.OverallB,
            OverallC = userStats.OverallC,
            OverallD = userStats.OverallD,
            OverallPerfect = userStats.OverallPerfect,
            OverallHits = userStats.OverallHits,
            Overall300 = userStats.Overall300,
            Overall100 = userStats.Overall100,
            Overall50 = userStats.Overall50,
            OverallGeki = userStats.OverallGeki,
            OverallKatu = userStats.OverallKatu,
            OverallMiss = userStats.OverallMiss
        };
    }
}