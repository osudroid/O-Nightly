using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public sealed class View_UserInfo_UserClassifications: IViewUserInfoUserClassificationsReadonly {
    public long UserId { get; set; }
    public bool CoreDeveloper { get; set; }
    public bool Developer { get; set; }
    public bool Contributor { get; set; }
    public bool Supporter { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Email { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public DateTime RegisterTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    public string Region { get; set; } = "";
    public bool Active { get; set; }
    public bool Banned { get; set; }
    public bool RestrictMode { get; set; }
    public bool Archived { get; set; }
    public DateTime UsernameLastChange { get; set; }
    public string LatestIp { get; set; } = "";
    public string PatronEmail { get; set; } = "";
    public bool PatronEmailAccept { get; set; }
}