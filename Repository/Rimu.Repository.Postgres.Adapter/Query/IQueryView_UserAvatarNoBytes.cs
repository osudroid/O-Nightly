using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryView_UserAvatarNoBytes {
    public Task<ResultOk<View_UserAvatarNoBytes[]>> FindByUserIdAsync(long userId);
    public Task<ResultOk<Option<View_UserAvatarNoBytes>>> FindByUserIdAndHashAsync(long userId,string hash);
}