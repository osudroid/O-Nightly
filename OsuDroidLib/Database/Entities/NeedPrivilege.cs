using NPoco;

namespace OsuDroidLib.Database.Entities; 

[TableName("need_privilege")]
[ExplicitColumns]
public class NeedPrivilege {
    [Column("need_privilege_id")] public Guid NeedPrivilegeId { get; set; }
    [Column("name")] public string? Name { get; set; }
}