using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryUserSetting {
    public Task<ResultOk<Option<UserSetting>>> GetByUserIdAsync(long userId);
    public Task<ResultNone> InsertAsync(UserSetting userSetting);
    public Task<ResultNone> DeleteAsync(long userId);
    public Task<ResultNone> UpdateAsync(UserSetting userSetting);
}