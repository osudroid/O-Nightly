using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class UserInfo: IUserInfoReadonly {
    public required long UserId { get; set; }
    public required string Username { get; set; } = "";
    public required string Password { get; set; } = "";
    public required string? PasswordGen2 { get; set; }
    public required string Email { get; set; } = "";
    public required string DeviceId { get; set; } = "";
    public required DateTime RegisterTime { get; set; }
    public required DateTime LastLoginTime { get; set; }
    public required string Region { get; set; } = "";
    public required bool Active { get; set; }
    public required bool Banned { get; set; }
    public required bool Archived { get; set; }
    public required bool RestrictMode { get; set; }
    public required DateTime UsernameLastChange { get; set; }
    public required string LatestIp { get; set; } = "";
    public required string? PatronEmail { get; set; }
    public required bool PatronEmailAccept { get; set; }

    public class UserRank {
        public long GlobalRank { get; set; }
        public long CountryRank { get; set; }
    }
    
    public bool HasPasswordGen1 => this.Password is { Length: > 4 };
    public bool HasPasswordGen2 => this.PasswordGen2 is { Length: > 4 };
}