using NPoco;

namespace OsuDroidLib.Database.Entities; 

[TableName("need_privilege_privilege")]
[ExplicitColumns]
public class NeedPrivilegePrivilege {
    [Column("need_privilege_id")] public Guid NeedPrivilegeId { get; set; }
    [Column("privilege_id")] public Guid PrivilegeId { get; set; }
}