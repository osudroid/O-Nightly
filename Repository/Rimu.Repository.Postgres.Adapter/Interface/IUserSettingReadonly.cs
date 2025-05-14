namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IUserSettingReadonly {
    public long UserId { get; }
    public bool ShowUserClassifications { get; }
}