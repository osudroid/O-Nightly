namespace OsuDroidLib.Database.OldEntities;

public class bbl_user {
    public long id { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    public string? email { get; set; }
    public string? deviceId { get; set; }
    public long score { get; set; }
    public long playcount { get; set; }
    public long accuracy { get; set; }
    public DateTime regist_time { get; set; }
    public DateTime last_login_time { get; set; }
    public string? regist_ip { get; set; }
    public string? region { get; set; }
    public long active { get; set; }
    public long supporter { get; set; }
    public long banned { get; set; }
    public long restrict_mode { get; set; }
}