using System.Data.Common;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Query;
using Err = LamLibAllOver.ResultErr<string>;
using Ok = LamLibAllOver.Result<LamLibAllOver.Option<OsuDroidLib.Database.Entities.ResetPasswordKey>, string>;

namespace OsuDroidLib.Manager; 

public class ResetPasswordKeyManager {
    public static async ValueTask<Err> InsertAsync(DbConnection db, ResetPasswordKey entity) {
        return await QueryResetPasswordKey.InsertAsync(db, entity);
    }

    public static async ValueTask<Ok> FindByTokenId(DbConnection db, string tokenId, long userId) {
        return await QueryResetPasswordKey.FindByTokenAndUserId(db, tokenId, userId);
    }
    
    public static async ValueTask<Ok> FindActiveKeyByTokenAsync(DbConnection db, string tokenId, long userId) {
        return await QueryResetPasswordKey.FindByTokenAndUserIdFilterTimeGreaterOrEqual(db, tokenId, userId, DateTime.UtcNow - TimeSpan.FromMinutes(10));
    }

    public static async ValueTask<Err> DeleteByToken(DbConnection db, string tokenId, long userId) {
        return await QueryResetPasswordKey.DeleteByTokenAndUserId(db, tokenId, userId);
    }
    
    public static async ValueTask<Err> DeleteOldRowsAsync(DbConnection db) {
        return await QueryResetPasswordKey.DeleteAllOlderThenCreateTime(db, DateTime.UtcNow - TimeSpan.FromMinutes(10));
    }
}