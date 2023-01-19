using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_user_status")]
[ExplicitColumns]
[PrimaryKey(new[] { "id" }, AutoIncrement = true)]
public class BblUserStatus {
    [Column("id")] public long Id { get; set; }
    [Column("type")] public string? Type { get; set; }
    [Column("session")] public string? Session { get; set; }
    [Column("ip")] public string? Ip { get; set; }
    [Column("ua")] public string? Ua { get; set; }
    [Column("time")] public DateTime Time { get; set; }
}