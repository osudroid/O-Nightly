using Npgsql;
using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;

// ReSharper disable All
namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryView_Play_PlayStats {
    public Task<ResultOk<List<View_Play_PlayStats>>> GetAllAsync();
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetUserTopScoreAsync(long userId, string filename, string fileHash);
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetUserTopPpAsync(long userId, string filename, string fileHash);
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetByIdAsync(long playId);
    public Task<ResultOk<long>> GetUserMapRankAsync(long playId);
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreByIdAsync(long id);
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreByIdAndUserIdAsync(long id, long userId);
    public Task<ResultOk<Option<View_Play_PlayStats>>> GetPlayScoreOldesByUserIdAndHashAsync(long userId, string mapHash);
    public Task<ResultOk<IReadOnlyList<MapTopPlays>>> MapTopPlaysByFilenameAndHashAsync(string filename, string fileHash, int maxResult);
    public Task<ResultOk<IReadOnlyList<MapTopPlays>>> MapTopPlaysByFilenameAndHashAllAsync(string filename, string fileHash);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdAsync(long userId, int limit, long offset);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetLastPlayScoreFilterByUserIdAsync(long userId, int limit);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdFilterMarkAsync(long userId, int size, long offset, PlayStatsDto.EPlayScoreMark mark);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreFromUserIdWithPageAsync(long userId, long page, int pageSize);
    public Task<ResultOk<Dictionary<PlayStatsDto.EPlayScoreMark, long>>> CountMarkPlaysByUserIdAsync(long userId);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetTopScoreByHashAsync(string playHash, int limit);
    public Task<ResultOk<List<View_Play_PlayStats>>> GetTopPpByHashAsync(string playHash, int limit);
}