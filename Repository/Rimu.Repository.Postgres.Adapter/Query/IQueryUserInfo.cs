using LamLibAllOver;
using Npgsql;
using Rimu.Repository.Postgres.Adapter.Class;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryUserInfo {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public Task<ResultNone> InsertAsync(UserInfo userInfo);
    public Task<ResultOk<long>> GetNewUserIdAsync();
    public Task<ResultNone> InsertBulkAsync(UserInfo[] userInfo);
    public Task<ResultOk<long[]>> GetAllIdsAsync();
    public Task<ResultOk<List<UserInfo>>> GetAllAsync();
    public Task<ResultOk<Option<UserInfo>>> GetUserIdByUsernameAsync(string username);
    public Task<ResultOk<StatisticActiveUser>> GetStatisticActiveUserAsync();
    public Task<ResultOk<Option<UserInfo>>> GetByUserIdAsync(long userId);
    public Task<ResultOk<Option<UserInfo>>> GetByUsernameAsync(string username);
    public Task<ResultOk<Option<UserInfo>>> GetByEmailAsync(string email);
    public Task<ResultOk<Option<UserInfo>>> GetUsernameAndRegionByUserId(long userId);
    public Task<ResultOk<Option<UserInfo>>> GetUsernameByUserIdAsync(long userId);
    public Task<ResultNone> DeleteAsync(long userId);
    public Task<ResultOk<Option<UserInfo>>> GetIdUsernamePasswordByLowerUsernameAsync(string username);
    public Task<ResultOk<Option<UserInfo>>> GetLoginInfoByEmailAsync(string email);
    public Task<ResultOk<Option<UserInfo>>> GetLoginInfoByByUsernameAsync(string username);
    public Task<ResultNone> UpdateIpAndRegionAsync(long userId, string region, string latestIp);
    public Task<ResultNone> UpdateEmailByUserId(long userId, string email);
    public Task<ResultNone> UpdatePasswordByUserIdAsync(long userId, string password);
    public Task<ResultNone> UpdateUsernameByUserIdAsync(long userId, string username);
    public Task<ResultOk<Option<UserInfo>>> CheckPasswordGetIdAndUsernameAsync(string passwordHash);
    public Task<ResultOk<IEnumerable<UserInfo>>> GetEmailAndUsernameByEmailAndUsername(string email, string username);
    public Task<ResultOk<bool>> CheckExistByEmailAndUsername(string email, string username);
    public Task<ResultNone> UpdatePasswordAsync(long userId, string passwordGen1Hash, string passwordGen2Hash);
    public Task<ResultOk<DateTime>> UpdateLastLoginTimeAsync(long userId);
    public Task<ResultNone> SetAcceptPatreonEmailAsync(long userId, bool accept = true);
    public Task<ResultNone> SetPatreonEmailAsync(long userId, string email);
    public Task<ResultNone> SetActiveAsync(long userId, bool isActive);
}