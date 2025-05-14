// ReSharper disable InconsistentNaming
namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlay_PlayStats_UserInfoReadonly: IPlayReadonly, IPlayStatsReadonly, IUserInfoReadonly {
    public new long Id { get; }
    public new long UserId { get; }
}