using Rimu.Repository.Postgres.Adapter.Entities;

// ReSharper disable InconsistentNaming

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryView_UserInfo_UserStats {
    public Task<ResultOk<Option<View_UserInfo_UserStats>>> GetByUserIdAsync(long userId);
}