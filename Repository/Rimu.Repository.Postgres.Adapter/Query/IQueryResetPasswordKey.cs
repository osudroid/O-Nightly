using System.Data.Common;
using Rimu.Repository.Postgres.Adapter.Entities;
using Err = LamLibAllOver.ErrorHandling.SResultErr;
using Ok = LamLibAllOver.ErrorHandling.ResultOk<LamLibAllOver.ErrorHandling.Option<Rimu.Repository.Postgres.Adapter.Entities.ResetPasswordKey>>;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryResetPasswordKey {
    public Task<ResultNone> InsertAsync(ResetPasswordKey entity);

    public Task<ResultOk<Option<ResetPasswordKey>>> FindByTokenAndUserId(string tokenId, long userId);

    public Task<ResultOk<Option<ResetPasswordKey>>> FindByTokenAndUserIdFilterTimeGreaterOrEqual(string tokenId, long userId, DateTime createTime);

    public Task<ResultNone> DeleteByTokenAndUserId(string tokenId, long userId);

    public Task<ResultNone> DeleteAllOlderThenCreateTime(DateTime createTime);
}