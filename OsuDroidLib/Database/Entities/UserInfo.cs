using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("UserInfo")]
public class UserInfo {
    [ExplicitKey] public long UserId { get; set; }
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

    public class UserRank {
        public long GlobalRank { get; set; }
        public long CountryRank { get; set; }
    }
}