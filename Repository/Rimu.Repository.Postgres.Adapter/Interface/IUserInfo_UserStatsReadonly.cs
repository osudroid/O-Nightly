// ReSharper disable InconsistentNaming
namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IUserInfo_UserStatsReadonly: IUserInfoReadonly, IUserStatsReadonly {
    public new long UserId { get; set; }
}