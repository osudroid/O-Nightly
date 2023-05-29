using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities; 

[Table("NeedPrivilege")]
public class NeedPrivilege {
    [ExplicitKey]
    public Guid NeedPrivilegeId { get; set; }
    public string? Name { get; set; }
}