using NPoco;

namespace OsuDroidLib.Database.Entities;
public class Privilege {
    [Column("id")] public Guid Id { get; set; }
    [Column("name")] public string? Name { get; set; }
    [Column("description")] public string? Description { get; set; }
}