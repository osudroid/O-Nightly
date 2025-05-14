namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IUserInfoReadonly {
    public long UserId { get; }
    public string Username { get; }
    public string Password { get; }
    public string Email { get; }
    public string DeviceId { get; }
    public DateTime RegisterTime { get; }
    public DateTime LastLoginTime { get; }
    public string Region { get; }
    public bool Active { get; }
    public bool Banned { get; }
    public bool RestrictMode { get; }
    public bool Archived { get; }
    public DateTime UsernameLastChange { get; }
    public string LatestIp { get; }
    public string? PatronEmail { get; }
    public bool PatronEmailAccept { get; }
}