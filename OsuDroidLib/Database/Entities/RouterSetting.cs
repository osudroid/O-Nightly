using NPoco;
namespace OsuDroidLib.Database.Entities; 

public class RouterSetting {
    [Column("path")] public string? Path { get; set; }
    [Column("need_privilege")] public Guid NeedPrivilege { get; set; }
    [Column("need_cookie")] public bool NeedCookie { get; set; }
    [Column("need_cookie_manager")] public string? NeedCookieManager { get; set; }
}