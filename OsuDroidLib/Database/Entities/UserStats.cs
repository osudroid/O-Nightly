using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("UserStats")]
public class UserStats {
    [ExplicitKey]
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
    public long OverallHits { get; set; }
    public long Overall300 { get; set; }
    public long Overall100 { get; set; }
    public long Overall50 { get; set; }
    public long OverallGeki { get; set; }
    public long OverallKatu { get; set; }
    public long OverallMiss { get; set; }


    /// <summary> Need OverallAccuracy And Playcount </summary>
    /// <returns></returns>
    public long FormatAccuracy() {
        return FormatAccuracy(OverallPlaycount, OverallAccuracy);
    }

    public static long FormatAccuracy(long playcount, long overallAccuracy) {
        return playcount == 0 ? 100000 : overallAccuracy / playcount;
    }

}