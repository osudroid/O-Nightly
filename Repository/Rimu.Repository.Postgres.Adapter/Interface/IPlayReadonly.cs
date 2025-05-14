namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlayReadonly {
    public long Id { get; }
    public long UserId { get; }
    public string Filename { get; }
    public string FileHash { get; }
}