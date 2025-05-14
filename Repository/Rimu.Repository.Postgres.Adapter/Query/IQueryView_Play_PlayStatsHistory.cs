using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryView_Play_PlayStatsHistory {
    public Task<ResultOk<List<View_Play_PlayStatsHistory>>> GetAllAsync();
}