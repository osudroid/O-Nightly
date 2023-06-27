using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("UserGroupPrivilege")]
public class UserGroupPrivilege {
    [ExplicitKey] public long UserId { get; set; }
    [ExplicitKey] public Guid GroupPrivilegeId { get; set; }
}