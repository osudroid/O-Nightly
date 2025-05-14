using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryLog {
    public Task<ResultNone> InsertAsync(Log log);
    public Task<ResultNone> InsertBulkAsync(Log[] log);
}