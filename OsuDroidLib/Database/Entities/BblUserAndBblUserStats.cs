using NPoco;

namespace OsuDroidLib.Database.Entities;

public class BblUserAndBblUserStats {
    [Column("id")] public long Id { get; set; }

    /// <summary> varchar(64) </summary>
    [Column("username")]
    public string? Username { get; set; }

    /// <summary> varchar(32) </summary>
    [Column("password")]
    public string? Password { get; set; }

    /// <summary> varchar(128) </summary>
    [Column("email")]
    public string? Email { get; set; }

    /// <summary> varchar(255) </summary>
    [Column("deviceid")]
    public string? Deviceid { get; set; }

    [Column("regist_time")] public DateTime RegistTime { get; set; }
    [Column("last_login_time")] public DateTime LastLoginTime { get; set; }

    /// <summary> varchar(16) </summary>
    [Column("latest_ip")]
    public string? LatestIp { get; set; }

    /// <summary> varchar(3) </summary>
    [Column("region")]
    public string? Region { get; set; }

    [Column("active")] public bool Active { get; set; }

    [Column("supporter")] public bool Supporter { get; set; }

    [Column("banned")] public bool Banned { get; set; }

    [Column("restrict_mode")] public bool RestrictMode { get; set; }

    [Column("username_last_change")] public DateTime UsernameLastChange { get; set; }
    [Column("patron_email")] public string? PatronEmail { get; set; }
    [Column("patron_email_accept")] public bool PatronEmailAccept { get; set; }


    [Column("uid")] public long Uid { get; set; }
    [Column("overall_playcount")] public long OverallPlaycount { get; set; }
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
    [Column("overall_perfect")] public long OverallPerfect { get; set; }
    [Column("overall_300")] public long Overall300 { get; set; }
    [Column("overall_100")] public long Overall100 { get; set; }
    [Column("overall_50")] public long Overall50 { get; set; }
    [Column("overall_geki")] public long OverallGeki { get; set; }
    [Column("overall_katu")] public long OverallKatu { get; set; }
    [Column("overall_miss")] public long OverallMiss { get; set; }

    public long FormatAccuracy() {
        return BblUserStats.FormatAccuracy(OverallPlaycount, OverallAccuracy);
    }
}