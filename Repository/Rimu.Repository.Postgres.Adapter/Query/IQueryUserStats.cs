using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryUserStats {
    public Task<ResultOk<long>> GetUserRank(long userId);
    public Task<ResultOk<Option<UserStats>>> GetByUserIdAsync(long userId);
    public Task<ResultNone> InsertAsync(UserStats userStats);
    public Task<ResultNone> InsertBulkAsync(UserStats[] userStatsList);
    public Task<ResultNone> InsertOrUpdateBulkAsync(UserStats[] userStatsList);
    public Task<ResultNone> UpdateStatsFromPlayStatsAsync(long userId, PlayStatsDto now, PlayStatsDto? old = null);
    public Task<ResultOk<Option<UserStats>>> GetBblUserStatsByUserIdAsync(long userId);
    public Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardFilterCountry(int limit, string countryNameShort);
    public Task<ResultOk<Option<LeaderBoardUser>>> LeaderBoardUserRank(long userId);
    public Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardNoFilter(int limit);
    public Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardSearchUser(long limit, string query);
    public Task<ResultOk<IEnumerable<LeaderBoardUser>>> LeaderBoardSearchUser(long limit, string query, string countryNameShortInfo);
    public Task<ResultOk<Option<long>>> UserRankByUserIdScopeCountryAsync(long userId, string countryNameShortInfo);
    
}