using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("GroupPrivilege")]
public class GroupPrivilege {
    [ExplicitKey] public Guid GroupPrivilegeId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}