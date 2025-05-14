using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Enum;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryTokenWithGroup {
    public Task<ResultNone> InsertAsync(TokenWithGroup value);
    public Task<ResultNone> DeleteAsync(ETokenGroup tokenGroup, string token);
    public Task<ResultOk<Option<TokenWithGroup>>> FindByTokenGroupAndTokenAsync(ETokenGroup tokenGroup, string token);
}