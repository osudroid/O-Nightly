using Npgsql;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryUserAvatar {
    public Task<ResultNone> InsertAsync(UserAvatar userAvatar);
    public Task<ResultNone> DeleteByUserIdAndHash(long userId, string hash);
    public Task<ResultNone> DeleteAllFromUserIdAsync(long userId);
    public Task<ResultOk<Option<UserAvatar>>> GetByUserIdAndSizeNoOriginalAsync(long userId, int size);
    public Task<ResultOk<Option<UserAvatar>>> GetByHashAsync(string hash);
    public Task<ResultOk<Option<UserAvatar>>> GetOriginalByUserIdAsync(long userId);
    public Task<ResultOk<Option<UserAvatar>>> GetLowByUserIdAsync(long userId);
    public Task<ResultOk<Option<UserAvatar>>> GetHighByUserIdAsync(long userId);
    public Task<ResultOk<Option<UserAvatar>>> GetByUserIdAndHash(long userId, string hash);
    public Task<ResultOk<IEnumerable<UserAvatar>>> GetManyUserIdAndHashByPixelSizeUserIdAsync(int pixelSize, IReadOnlyList<long> userIds);
}