using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryPlayStatsHistory {
    public Task<ResultOk<List<PlayStatsHistory>>> GetAllAsync();

    public Task<ResultOk<long>> InsertWithNewIdAsync(PlayStatsHistory playStatsHistory);

    public Task<ResultNone> InsertBulkWithNewIdAsync(PlayStatsHistory[] playStatsHistories);
}