using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_avatar_hash")]
[ExplicitColumns]
[PrimaryKey(new[] { "UserId", "Size" }, AutoIncrement = false)]
public class BblAvatarHash {
    [Column(Name = "user_id")] public long UserId { get; set; }
    [Column(Name = "size")] public int Size { get; set; }
    [Column(Name = "hash")] public string? Hash { get; set; }
}