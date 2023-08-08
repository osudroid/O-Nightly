using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryPatreonEmailToken {
    public static async ValueTask<ResultErr<string>> InsertAsync(NpgsqlConnection db, PatreonEmailToken value) {
        return await db.SafeExecuteAsync(@"
INSERT INTO PatreonEmailToken 
    (UserId, Token, CreateTime, Email) 
VALUES 
    (@UserId, @Token, @CreateTime, @Email);
", new {
            UserId = value.UserId, 
            Token = value.Token, 
            CreateTime = value.CreateTime, 
            Email = value.Email
        });
    }

    public static async ValueTask<ResultErr<string>> RemoveToOldRows(NpgsqlConnection db, DateTime limitCreateTime) {
        return await db.SafeExecuteAsync(@"
DELETE FROM PatreonEmailToken
WHERE CreateTime <= @limit
", new { limit = limitCreateTime });
    }
    
    public static async ValueTask<Result<Option<PatreonEmailToken>, string>>FindByTokenWithLimitTime(
        NpgsqlConnection db, Guid token, DateTime limitCreateTime) {

        return await db.SafeQueryFirstOrDefaultAsync<PatreonEmailToken>(@"
SELECT * 
FROM PatreonEmailToken
WHERE CreateTime <= @limit
  AND Token = @Token 
LIMIT 1
", new { limit = limitCreateTime, Token = token });
    }
}