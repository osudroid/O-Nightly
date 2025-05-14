using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryView_Play_PlayStats_UserInfo {
    public Task<ResultOk<View_Play_PlayStats_UserInfo[]>> BeatmapTopAsync(string filename, string filehash, long offset, int length);
    public Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> GetTopScoreByHashAsync(string playHash, int limit);
    public Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> GetTopPpByHashAsync(string playHash, int limit);
    public Task<ResultOk<Option<View_Play_PlayStats_UserInfo>>> GetTopPpByHashAndUserIdAsync(string playHash, long userId);
    public Task<ResultOk<Option<View_Play_PlayStats_UserInfo>>> GetTopScoreByHashAndUserIdAsync(string playHash, long userId);
    public Task<ResultOk<Option<BoxLong>>> GetRankSortPpByHashAndUserIdAsync(string playHash, long userId);
    public Task<ResultOk<Option<BoxLong>>> GetRankScoreSortByHashAndUserIdAsync(string playHash, long userId);
}