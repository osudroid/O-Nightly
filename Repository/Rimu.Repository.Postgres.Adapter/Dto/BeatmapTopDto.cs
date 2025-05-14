namespace Rimu.Repository.Postgres.Adapter.Dto;

public class BeatmapTopDto {
    public long UserId { get; set; }
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
    public DateTime UsernameLastChange { get; set; }
    public string LatestIp { get; set; } = "";
    public string PatronEmail { get; set; } = "";
    public bool PatronEmailAccept { get; set; }

    public long Id { get; set; }
    public string[]? Mode { get; set; }
    public long Score { get; set; }
    public long Combo { get; set; }
    public string? Mark { get; set; }
    public long Geki { get; set; }
    public long Perfect { get; set; }
    public long Katu { get; set; }
    public long Good { get; set; }
    public long Bad { get; set; }
    public long Miss { get; set; }
    public DateTime Date { get; set; }
    public long Accuracy { get; set; }

    public string FileHash { get; set; } = "";
    public string Filename { get; set; } = "";
}