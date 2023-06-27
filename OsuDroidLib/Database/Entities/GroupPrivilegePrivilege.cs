using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("GroupPrivilegePrivilege")]
public class GroupPrivilegePrivilege {
    [ExplicitKey] public Guid GroupPrivilegeId { get; set; }
    public bool ModeAllow { get; set; }
    [ExplicitKey] public Guid PrivilegeId { get; set; }
}