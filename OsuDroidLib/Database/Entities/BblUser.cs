using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_user")]
[ExplicitColumns]
[PrimaryKey(new[] { "id" }, AutoIncrement = true)]
public class BblUser {
    [Column("id")] public long Id { get; set; }

    /// <summary> varchar(64) </summary>
    [Column("username")]
    public string? Username { get; set; }

    /// <summary> varchar(32) </summary>
    [Column("password")]
    public string? Password { get; set; }

    /// <summary> varchar(128) </summary>
    [Column("email")]
    public string? Email { get; set; }

    /// <summary> varchar(255) </summary>
    [Column("deviceid")]
    public string? Deviceid { get; set; }

    [Column("regist_time")] public DateTime RegistTime { get; set; }
    [Column("last_login_time")] public DateTime LastLoginTime { get; set; }

    /// <summary> varchar(16) </summary>
    [Column("latest_ip")]
    public string? LatestIp { get; set; }

    /// <summary> varchar(3) </summary>
    [Column("region")]
    public string? Region { get; set; }

    [Column("active")] public bool Active { get; set; }
    [Column("banned")] public bool Banned { get; set; }
    [Column("restrict_mode")] public bool RestrictMode { get; set; }
    [Column("username_last_change")] public DateTime UsernameLastChange { get; set; }
    [Column("patron_email")] public string? PatronEmail { get; set; }
    [Column("patron_email_accept")] public bool PatronEmailAccept { get; set; }

    public class UserRank {
        [Column("global_rank")] public long globalRank { get; set; }
        [Column("country_rank")] public long CountryRank { get; set; }
    }
}