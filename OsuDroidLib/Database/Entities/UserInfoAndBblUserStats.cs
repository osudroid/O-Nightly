namespace OsuDroidLib.Database.Entities;

public class UserInfoAndBblUserStats {
    public long Id { get; set; }

    /// <summary> varchar(64) </summary>
    public string? Username { get; set; }

    /// <summary> varchar(32) </summary>
    public string? Password { get; set; }

    /// <summary> varchar(128) </summary>
    public string? Email { get; set; }

    /// <summary> varchar(255) </summary>
    public string? Deviceid { get; set; }

    public DateTime RegistTime { get; set; }
    public DateTime LastLoginTime { get; set; }

    /// <summary> varchar(16) </summary>
    public string? LatestIp { get; set; }

    /// <summary> varchar(3) </summary>
    public string? Region { get; set; }
    public bool Active { get; set; }
    public bool Supporter { get; set; }
    public bool Banned { get; set; }
    public bool RestrictMode { get; set; }
    public DateTime UsernameLastChange { get; set; }
    public string? PatronEmail { get; set; }
    public bool PatronEmailAccept { get; set; }
    public long Uid { get; set; }
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
    public long OverallPerfect { get; set; }
    public long Overall300 { get; set; }
    public long Overall100 { get; set; }
    public long Overall50 { get; set; }
    public long OverallGeki { get; set; }
    public long OverallKatu { get; set; }
    public long OverallMiss { get; set; }
}