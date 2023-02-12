using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_user_stats")]
[ExplicitColumns]
[PrimaryKey(new[] { "uid" }, AutoIncrement = true)]
public class BblUserStats {
    [Column("uid")] public long Uid { get; set; }
    [Column("playcount")] public long Playcount { get; set; }
    [Column("overall_score")] public long OverallScore { get; set; }
    [Column("overall_accuracy")] public long OverallAccuracy { get; set; }
    [Column("overall_combo")] public long OverallCombo { get; set; }
    [Column("overall_xss")] public long OverallXss { get; set; }
    [Column("overall_ss")] public long OverallSs { get; set; }
    [Column("overall_xs")] public long OverallXs { get; set; }
    [Column("overall_s")] public long OverallS { get; set; }
    [Column("overall_a")] public long OverallA { get; set; }
    [Column("overall_b")] public long OverallB { get; set; }
    [Column("overall_c")] public long OverallC { get; set; }
    [Column("overall_d")] public long OverallD { get; set; }
    [Column("overall_hits")] public long OverallHits { get; set; }
    [Column("overall_300")] public long Overall300 { get; set; }
    [Column("overall_100")] public long Overall100 { get; set; }
    [Column("overall_50")] public long Overall50 { get; set; }
    [Column("overall_geki")] public long OverallGeki { get; set; }
    [Column("overall_katu")] public long OverallKatu { get; set; }
    [Column("overall_miss")] public long OverallMiss { get; set; }

    public Result<long, string> GetUserRank(SavePoco db) {
        return GetUserRank(db, OverallScore, Uid);
    }

    public static Result<long, string> GetUserRank(SavePoco db, long score, long userId) {
        return db.FirstOrDefault<long>(@$"
SELECT user_rank
FROM (
    SELECT uid, rank() OVER (ORDER BY overall_score DESC, bu.last_login_time DESC) as user_rank
    FROM bbl_user_stats
    JOIN bbl_user bu on bbl_user_stats.uid = bu.id
    WHERE bu.banned = false) as t
WHERE uid = {userId};
");
    }

    /// <summary> Need OverallAccuracy And Playcount </summary>
    /// <returns></returns>
    public long FormatAccuracy() {
        return FormatAccuracy(Playcount, OverallAccuracy);
    }

    public static long FormatAccuracy(long playcount, long overallAccuracy) {
        return playcount == 0 ? 100000 : overallAccuracy / playcount;
    }

    public static void UpdateStatsFromScore(SavePoco db, long uid, BblScore now, BblScore? old = null) {
        var dif = old is null ? now : now - old;
        var playcount = old is null ? 1 : 0;

        db.Execute(@$"
UPDATE bbl_user_stats 
SET
    playcount = playcount + {playcount}, 
    overall_score = overall_score + {dif.Score}, 
    overall_accuracy = overall_accuracy + {dif.Accuracy}, 
    overall_combo = overall_combo + {dif.Combo}, 
    overall_xss = overall_xss + {dif.EqAsInt(BblScore.EMark.XSS)}, 
    overall_ss = overall_ss + {dif.EqAsInt(BblScore.EMark.SS)}, 
    overall_xs = overall_xs + {dif.EqAsInt(BblScore.EMark.XS)}, 
    overall_s = overall_s + {dif.EqAsInt(BblScore.EMark.S)}, 
    overall_a = overall_a + {dif.EqAsInt(BblScore.EMark.A)}, 
    overall_b = overall_b + {dif.EqAsInt(BblScore.EMark.B)}, 
    overall_c = overall_c + {dif.EqAsInt(BblScore.EMark.C)}, 
    overall_d = overall_d + {dif.EqAsInt(BblScore.EMark.D)}, 
    overall_hits = overall_hits + {dif.GetValue(BblScore.EBblScore.Hits)},  
    overall_300 = overall_300 + {dif.GetValue(BblScore.EBblScore.N300)}, 
    overall_100 = overall_100 + {dif.GetValue(BblScore.EBblScore.N100)}, 
    overall_50 = overall_50 + {dif.GetValue(BblScore.EBblScore.N50)}, 
    overall_geki = overall_geki + {dif.Geki}, 
    overall_katu = overall_katu + {dif.Katu}, 
    overall_miss = overall_miss + {dif.Miss}
WHERE uid = {uid}
");
    }
}