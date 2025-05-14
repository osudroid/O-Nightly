using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQuerySetting {
    public Task<ResultOk<Option<Setting>>> GetSetting(string mainKey, string subKey);

    public Task<ResultOk<IEnumerable<Setting>>> GetSettingsAsync(string mainKey);

    public Task<ResultOk<IEnumerable<Setting>>> GetAllAsync();
}