using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryPlay {
    public Task<ResultOk<long>> InsertIfNotExistAsync(Play play);
    
    public Task<ResultOk<Option<Play>>> GetByIdAsync(long id);
    
    public Task<ResultOk<Option<Play>>> GetByUserIdFilenameFileHashAsync(long id, string filename, string fileHash);
    
    public Task<ResultNone> InsertBulkAsync(Play[] plays);
}