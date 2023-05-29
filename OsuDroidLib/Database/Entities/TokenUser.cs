using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("TokenUser")]
public class TokenUser {
    public Guid TokenId { get; set; }
    public long UserId { get; set; }
    public DateTime CreateDate { get; set; }
}