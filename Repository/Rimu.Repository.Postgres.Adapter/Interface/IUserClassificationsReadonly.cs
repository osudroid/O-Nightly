namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IUserClassificationsReadonly {
    public long UserId { get; }
    public bool CoreDeveloper { get; }
    public bool Developer { get; }
    public bool Contributor { get; }
    public bool Supporter { get; }
}