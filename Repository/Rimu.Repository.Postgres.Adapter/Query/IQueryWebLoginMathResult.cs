using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryWebLoginMathResult {
    public Task<ResultNone> AddWebLoginTokenAsync(WebLoginMathResult webLoginMathResult);

    public Task<ResultNone> DeleteWebLoginTokenAsync(Guid id);

    public Task<ResultOk<Option<WebLoginMathResult>>> GetWebLoginTokenAsync(Guid id);

    public Task<ResultNone> DeleteOldTokens(TimeSpan timeSpan);
}