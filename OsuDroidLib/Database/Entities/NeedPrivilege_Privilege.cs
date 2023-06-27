using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

/// <summary>
/// Table NeedPrivilege_Privilege
/// </summary>
[Table("NeedPrivilege_Privilege")]
public class NeedPrivilege_Privilege {
    [ExplicitKey] public Guid NeedPrivilegeId { get; set; }
    [ExplicitKey] public Guid PrivilegeId { get; set; }
}