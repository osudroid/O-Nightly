using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("UserInfo")]
public class UserInfo {
    [ExplicitKey]
    public long UserId { get; set; }

    /// <summary> varchar(64) </summary>
    public string? Username { get; set; }

    /// <summary> varchar(32) </summary>
    public string? Password { get; set; }

    /// <summary> varchar(128) </summary>
    public string? Email { get; set; }

    /// <summary> varchar(255) </summary>
    public string? DeviceId { get; set; }

    public DateTime RegisterTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    /// <summary> varchar(3) </summary>
    public string? Region { get; set; }
    public bool Active { get; set; }
    public bool Banned { get; set; }
    public bool RestrictMode { get; set; }
    public DateTime UsernameLastChange { get; set; }
    /// <summary> varchar(16) </summary>
    public string? LatestIp { get; set; }
    public string? PatronEmail { get; set; }
    public bool PatronEmailAccept { get; set; }
    
    public class UserRank {
        public long GlobalRank { get; set; }
        public long CountryRank { get; set; }
    }
}