using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQuerySettingsHot {
    public Task<ResultOk<Option<SettingsHot>>> GetByMainKeySubKeyAsync(string mainKey, string subKey);
    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_PathAsync();
    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_UpdateUrlAsync();
    public Task<ResultOk<Option<SettingsHot>>> GetChangeLogs_VersionAsync();
    public Task<ResultOk<List<SettingsHot>>> GetAllAsync();
}