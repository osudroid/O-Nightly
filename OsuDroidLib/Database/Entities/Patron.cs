using Dapper.Contrib.Extensions;

namespace OsuDroidLib.Database.Entities;

[Table("Patron")]
public class Patron {
    [ExplicitKey] public string? PatronEmail { get; set; }
    public bool ActiveSupporter { get; set; }
}