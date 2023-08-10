using System.Data.Common;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using Err = LamLibAllOver.ResultErr<string>;
using Ok = LamLibAllOver.Result<LamLibAllOver.Option<OsuDroidLib.Database.Entities.ResetPasswordKey>, string>;

namespace OsuDroidLib.Query;

public class QueryResetPasswordKey {
    public static async ValueTask<Err> InsertAsync(DbConnection db, ResetPasswordKey entity) {
        return await db.SafeExecuteAsync(@"
INSERT INTO ResetPasswordKey 
(Token, UserId, CreateTime) 
VALUES (@Token, @UserId, @CreateTime); 
", new {
                entity.Token,
                entity.UserId,
                entity.CreateTime
            }
        );
    }

    public static async ValueTask<Ok> FindByTokenAndUserId(DbConnection db, string tokenId, long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<ResetPasswordKey>(@"
SELECT * FROM ResetPasswordKey WHERE Token = @Token AND UserId = @UserId LIMIT 1
", new { Token = tokenId, UserId = userId }
        );
    }

    public static async ValueTask<Ok> FindByTokenAndUserIdFilterTimeGreaterOrEqual(
        DbConnection db,
        string tokenId,
        long userId,
        DateTime createTime) {
        return await db.SafeQueryFirstOrDefaultAsync<ResetPasswordKey>(@"
SELECT * FROM ResetPasswordKey WHERE Token = @Token AND UserId = @UserId AND CreateTime <= @CreateTime LIMIT 1
", new { Token = tokenId, UserId = userId, CreateTime = createTime }
        );
    }

    public static async ValueTask<Err> DeleteByTokenAndUserId(DbConnection db, string tokenId, long userId) {
        return await db.SafeExecuteAsync(@"
Delete FROM ResetPasswordKey WHERE Token = @Token AND UserId = @UserId
", new { Token = tokenId, UserId = userId }
        );
    }

    public static async ValueTask<Err> DeleteAllOlderThenCreateTime(DbConnection db, DateTime createTime) {
        return await db.SafeExecuteAsync(@"
Delete FROM ResetPasswordKey WHERE CreateTime > @CreateTime
", new { CreateTime = createTime }
        );
    }
}