using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryTokenUser {
    public Task<ResultOk<IEnumerable<TokenUser>>> GetAllTokensAsync();
    public Task<ResultNone> CreateOrUpdateAsync(DateTime createDay, long userId, string token);
    public Task<ResultNone> CreateOrUpdateAsync(TokenUser tokenUser);
    public Task<ResultOk<Option<TokenUser>>> GetByTokenAsync(string tokenId);
    public Task<ResultNone> DeleteOlderEqThen(DateTime time);
    public Task<ResultNone> DeleteManyByUserIdAsync(long userId);
    public Task<ResultNone> DeleteByTokenIdAsync(string tokenId);
    public Task<ResultNone> UpdateCreateTimeAsync(string token, DateTime createDate);
}