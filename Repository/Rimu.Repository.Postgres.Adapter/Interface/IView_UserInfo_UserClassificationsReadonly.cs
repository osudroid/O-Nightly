namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IViewUserInfoUserClassificationsReadonly: IUserInfoReadonly, IUserClassificationsReadonly {
    public new long UserId { get; }
}