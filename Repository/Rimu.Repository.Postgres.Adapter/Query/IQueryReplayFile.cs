using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryReplayFile {
    public Task<ResultOk<long>> InsertAsync(byte[] odr);
    public Task<ResultNone> BulkInsertWithIdsAsync(ReplayFile[] replayFiles);

    public Task<ResultOk<Option<ReplayFile>>> GetByIdAsync(long id);
    public Task<ResultOk<long[]>> GetAllIdsAsync();
}