using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class Play: IPlayReadonly {
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Filename { get; set; } = "";
    public string FileHash { get; set; } = "";
}