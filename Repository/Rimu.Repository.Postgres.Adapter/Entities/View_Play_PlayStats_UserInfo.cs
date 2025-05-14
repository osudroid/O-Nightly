
// ReSharper disable InconsistentNaming

using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class View_Play_PlayStats_UserInfo: IPlay_PlayStatsReadonly {
    public required long Id { get; set; }
    public required long UserId { get; set; }
    public required string Filename { get; set; } = "";
    public required string FileHash { get; set; } = "";
    public required string[] Mode { get; set; } = Array.Empty<string>();
    public required long Score { get; set; }
    public required long Combo { get; set; }
    public required string Mark { get; set; } = "";
    public required long Geki { get; set; }
    public required long Perfect { get; set; }
    public required long Katu { get; set; }
    public required long Good { get; set; }
    public required long Bad { get; set; }
    public required long Miss { get; set; }
    public required DateTime Date { get; set; }
    public required double Accuracy { get; set; }
    public required double Pp { get; set; }
    public required long? ReplayFileId { get; set; }
    public required string? Username { get; set; }
    public required string? Password { get; set; }
    public required string? Email { get; set; }
    public required string? DeviceId { get; set; }
    public required DateTime RegisterTime { get; set; }
    public required DateTime LastLoginTime { get; set; }
    public required string? Region { get; set; }
    public required bool Active { get; set; }
    public required bool Banned { get; set; }
    public required bool RestrictMode { get; set; }
    public required DateTime UsernameLastChange { get; set; }
    public required string? LatestIp { get; set; }
    public required string? PatronEmail { get; set; }
    public required bool PatronEmailAccept { get; set; }
    
    public bool IsBetterThen(IPp other) => Pp > other.Pp;
}