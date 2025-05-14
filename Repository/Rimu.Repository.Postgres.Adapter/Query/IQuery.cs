using LamLibAllOver.ErrorHandling;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQuery {
    public Task<ResultOk<List<BeatmapTopDto>>> GetBeatmapTop(string filename, string fileHash);

    public Task<ResultOk<List<View_Play_PlayStats_UserInfo>>> PlayRecentFilterByAsync(
        string filterPlays,
        string orderBy,
        int limit,
        int startAt);

    public Task<ResultOk<Option<UserInfo.UserRank>>> GetUserRankAsync(long userId, long userOverallScore);
    
    public Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUsersCountry(int limit, string countryNameShortToUpper);

    public Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUserLikeUserQuery(int limit, string likeUserQuery);

    public Task<ResultOk<List<LeaderBoardUser>>> LeaderBoardUserNormal(int limit);

    public Task<ResultOk<Option<LeaderBoardUser>>> LeaderBoardUserSingleUser(long userId);
}