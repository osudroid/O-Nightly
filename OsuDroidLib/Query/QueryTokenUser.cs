using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryTokenUser {
    public static async Task<Result<IEnumerable<TokenUser>, string>> GetAllTokensAsync(NpgsqlConnection db) {
        return await db.SafeQueryAsync<TokenUser>("SELECT * FROM TokenUser");
    }

    public static async Task<ResultErr<string>> CreateOrUpdateAsync(NpgsqlConnection db, DateTime createDay, long userId, Guid token) {
        var sql = @$"
INSERT 
INTO TokenUser (TokenId, UserId, CreateDate) 
VALUES (@token, {userId}, @createDay) 
ON CONFLICT (@token) DO UPDATE 
set CreateDate = @createDay,
    UserId = {userId}
";
        return await db.SafeQueryAsync(sql, new { createDay = createDay, token = token });
    }
    
    public static async Task<Result<Option<TokenUser>, string>> GetByTokenAsync(NpgsqlConnection db, Guid token) {
        return await db.SafeQueryFirstOrDefaultAsync<TokenUser>(@"
SELECT * 
FROM TokenUser
WHERE TokenId = @id
", new { id = token });
    }
    
    public static async Task<ResultErr<string>> DeleteOlderEqThen(NpgsqlConnection db, DateTime time) {
        return await db.SafeQueryAsync<TokenUser>(@$"
DELETE 
FROM TokenUser
WHERE CreateDate <= '{Time.ToScyllaString(time)}'
");
    }

    public static async Task<ResultErr<string>> DeleteManyByUserIdAsync(NpgsqlConnection db, long userId) {
        return await db.SafeQueryAsync<TokenUser>(@$"
DELETE 
FROM TokenUser
WHERE UserId = @UserId
", new { UserId = userId });     
    }
    
    public static async Task<ResultErr<string>> DeleteByTokenIdAsync(NpgsqlConnection db, Guid tokenId) {
        return await db.SafeQueryAsync<TokenUser>(@$"
DELETE 
FROM TokenUser
WHERE TokenId = @TokenId
", new { TokenId = tokenId });     
    }

    public static async Task<ResultErr<string>> UpdateCreateTimeAsync(NpgsqlConnection db, Guid token, DateTime createDate) {
        return await db.SafeQueryAsync(@$"
UPDATE TokenUser
SET CreateDate = '{Time.ToScyllaString(createDate)}'
WHERE TokenId = @Token 
", new { Token = token });
    }
}