namespace OsuDroidLib.Database.Entities;

public class UserInfoAndBblUserStats {
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? DeviceId { get; set; }

    public DateTime RegisterTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    public string? Region { get; set; }
    public bool Active { get; set; }
    public bool Banned { get; set; }
    public bool RestrictMode { get; set; }
    public DateTime UsernameLastChange { get; set; }
    public string? LatestIp { get; set; }
    public string? PatronEmail { get; set; }
    public bool PatronEmailAccept { get; set; }
    
    
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
}