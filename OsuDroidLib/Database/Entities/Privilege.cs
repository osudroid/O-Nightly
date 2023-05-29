using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;
[Table("Privilege")]
public class Privilege {
    [ExplicitKey]
    public Guid PrivilegeId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}