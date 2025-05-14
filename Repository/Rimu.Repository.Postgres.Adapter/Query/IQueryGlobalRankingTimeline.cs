using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryGlobalRankingTimeline {
    public Task<ResultOk<List<GlobalRankingTimeline>>> GetAllAsync();
    
    public Task<ResultOk<Option<GlobalRankingTimeline>>> Now(long userId);
    
    public Task<ResultOk<IReadOnlyList<GlobalRankingTimeline>>> BuildTimeLineAsync(long userId, DateTime startAt);
    
    public Task<ResultNone> DeleteAllRankingByAllUserId(long userId);
    
    public Task<ResultNone> InsertBulkAsync(GlobalRankingTimeline[] globalRankingTimelines);

    public Task<ResultOk<GlobalRankingTimeline[]>> GetRangeByUserIdAndStartAtAsync(long userId, DateOnly startAt);
}