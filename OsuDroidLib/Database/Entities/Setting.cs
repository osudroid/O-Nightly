using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities; 

[Table("Setting")]
public class Setting {
    [ExplicitKey] public string? MainKey { get; set; }
    [ExplicitKey] public string? SubKey { get; set; }
    public string? Value { get; set; }
}