
using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities; 

[Table("RouterSetting")]
public class RouterSetting {
    [ExplicitKey]
    public string? Path { get; set; }
    public Guid NeedPrivilege { get; set; }
    public bool NeedCookie { get; set; }
    public string? NeedCookieHandler { get; set; }
}