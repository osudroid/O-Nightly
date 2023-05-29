using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("UserAvatar")]
public class UserAvatar {
    [ExplicitKey]
    public long UserId { get; set; }
    [ExplicitKey]
    public string? Hash { get; set; }
    public string? TypeExt { get; set; }
    public int PixelSize { get; set; }
    public bool Animation { get; set; }
}