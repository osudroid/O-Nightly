namespace Rimu.Repository.Postgres.Adapter.Entities;

public class ReplayFile {
    public required long Id { get; set; }
    public required byte[] Odr { get; set; } = [];
}