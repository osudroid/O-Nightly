using NPoco;

namespace OsuDroidLib.Database.OldEntities;

public class bbl_user {
    [Column("id")] public long Id { get; set; }
    [Column("username")] public string? Username { get; set; }
    [Column("password")] public string? Password { get; set; }
    [Column("email")] public string? Email { get; set; }
    [Column("deviceid")] public string? DeviceId { get; set; }
    [Column("score")] public long Score { get; set; }
    [Column("playcount")] public long Playcount { get; set; }
    [Column("accuracy")] public long Accuracy { get; set; }
    [Column("regist_time")] public DateTime RegistTime { get; set; }
    [Column("last_login_time")] public DateTime LastLoginTime { get; set; }
    [Column("regist_ip")] public string? RegistIp { get; set; }
    [Column("region")] public string? Region { get; set; }
    [Column("active")] public long Active { get; set; }
    [Column("supporter")] public long Supporter { get; set; }
    [Column("banned")] public long Banned { get; set; }
    [Column("restrict_mode")] public long RestrictMode { get; set; }
}