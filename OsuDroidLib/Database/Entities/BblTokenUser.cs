using NPoco;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_token_user")]
[ExplicitColumns]
[PrimaryKey(new[] { "token_id" }, AutoIncrement = false)]
public class BblTokenUser {
    [Column("token_id")] public Guid TokenId { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("create_date")] public DateTime CreateDate { get; set; }
}