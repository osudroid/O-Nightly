using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query;

public static class QueryUserAvatar {
    public static async Task<ResultErr<string>> InsertAsync(NpgsqlConnection db, UserAvatar userAvatar) {
        return await db.SafeInsertAsync(userAvatar);
    }

    public static async Task<ResultErr<string>> DeleteByUserIdAndHash(
        NpgsqlConnection db, long userId, string hash) {
        return await db.SafeQueryAsync(@"
DELETE 
FROM UserAvatar 
WHERE UserId = @UserId
AND Hash = @Hash
", new { UserId = userId, Hash = hash });
    }

    public static async Task<ResultErr<string>> DeleteAllFromUserIdAsync(
        NpgsqlConnection db, long userId) {
        return await db.SafeQueryAsync(@"
DELETE 
FROM UserAvatar 
WHERE UserId = @UserId
", new { UserId = userId });
    }

    public static async Task<Result<Option<UserAvatar>, string>> GetByUserIdAndSizeNoOriginalAsync(
        NpgsqlConnection db, long userId, int size) {
        var sql = @"
SELECT * FROM UserAvatar
WHERE UserId = @UserId
AND PixelSize = @PixelSize
AND Original = false
";

        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql,
            new { UserId = userId, PixelSize = size }));
    }

    public static async Task<Result<Option<UserAvatar>, string>> GetByHashAsync(
        NpgsqlConnection db, string hash) {
        var sql = @"
SELECT * FROM UserAvatar
WHERE Hash = @Hash
";

        return (await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql, new { Hash = hash }));
    }

    public static async Task<Result<Option<UserAvatar>, string>> GetOriginalByUserIdAsync(NpgsqlConnection db,
        long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(@$"
SELECT * 
FROM UserAvatar
WHERE UserId = {userId}
AND Original = true
");
    }

    public static async Task<Result<Option<UserAvatar>, string>> GetLowByUserIdAsync(NpgsqlConnection db, long userId) {
        var sql = @$"
SELECT * 
FROM UserAvatar
WHERE UserId = {userId}
AND Original = false
ORDER BY PixelSize ASC
";
        return await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql);
    }

    public static async Task<Result<Option<UserAvatar>, string>>
        GetHighByUserIdAsync(NpgsqlConnection db, long userId) {
        var sql = @$"
SELECT * 
FROM UserAvatar
WHERE UserId = {userId}
AND Original = false
ORDER BY PixelSize DESC 
";
        return await db.SafeQueryFirstOrDefaultAsync<UserAvatar>(sql);
    }

    public static async Task<Result<IEnumerable<UserAvatar>, string>> GetManyUserIdAndHashByPixelSizeUserIdAsync(
        NpgsqlConnection db, int pixelSize, IReadOnlyList<long> userIds) {
        return await db.SafeQueryAsync<UserAvatar>(@$"
SELECT UserId, Hash
FROM UserAvatar
WHERE PixelSize = {pixelSize}
AND UserId in @UserIds
", new { UserIds = userIds });
    }
}