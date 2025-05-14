using Rimu.Repository.Postgres.Adapter.Interface;

// ReSharper disable InconsistentNaming

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class View_UserInfo_UserStats: IUserInfo_UserStatsReadonly {
    public required long UserId { get; set; }
    public required string Username { get; set; } = "";
    public required string Password { get; set; } = "";
    public required string Email { get; set; } = "";
    public required string DeviceId { get; set; } = "";

    public required DateTime RegisterTime { get; set; }
    public required DateTime LastLoginTime { get; set; }
    public required string Region { get; set; } = "";
    public required bool Active { get; set; }
    public required bool Banned { get; set; }
    public required bool RestrictMode { get; set; }
    public bool Archived { get; set; }
    public required DateTime UsernameLastChange { get; set; }
    public required string LatestIp { get; set; } = "";
    public required string PatronEmail { get; set; } = "";
    public required bool PatronEmailAccept { get; set; }
    
    public required long OverallPlaycount { get; set; }
    public required long OverallScore { get; set; }
    public required double OverallAccuracy { get; set; }
    public required double OverallPp { get; set; }
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
}