using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryPlayStats {
    public Task<ResultOk<List<PlayStats>>> GetAllAsync();
    
    public Task<ResultNone> InsertAsync(PlayStats playStats);

    public Task<ResultNone> UpdateAsync(PlayStats playStats);
    public Task<ResultNone> DeleteByIdAsync(long id);

    public Task<ResultNone> InsertBulkAsync(PlayStats[] playStatss);
}