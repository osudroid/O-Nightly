using NPoco;

namespace OsuDroidLib.Database.Entities; 


public class Setting {
    [Column("name")] public string? Name { get; set; }
    [Column("value")] public string? Value { get; set; }
}