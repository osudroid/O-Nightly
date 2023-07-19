using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using Db = Npgsql.NpgsqlConnection; 
namespace OsuDroidLib.Query; 

public static class QueryWebLoginMathResult {
    public static async ValueTask<ResultErr<string>> AddWebLoginTokenAsync(Db db, WebLoginMathResult webLoginMathResult) {
        return await db.SafeExecuteAsync(@"
INSERT INTO WebLoginMathResult (WebLoginMathResultId, CreateTime, MathResult) VALUES (@WebLoginMathResultId, @CreateTime, @UserId)
",webLoginMathResult);
    }

    public static async ValueTask<ResultErr<string>> DeleteWebLoginTokenAsync(Db db, Guid id) {

        return await db.SafeExecuteAsync(@"
DELETE 
FROM WebLoginMathResult 
WHERE WebLoginMathResultId = @WebLoginMathResultId
", new { WebLoginMathResultId = id });
        
    }

    public static async ValueTask<Result<Option<WebLoginMathResult>, string>> GetWebLoginTokenAsync(Db db, Guid id) {
        var result = await db.SafeQueryFirstOrDefaultAsync<WebLoginMathResult>(@"
SELECT *
FROM WebLoginMathResult 
WHERE WebLoginMathResultId = @WebLoginMathResultId
", new { WebLoginMathResultId = id });

        return result;
    }

    public static async ValueTask<ResultErr<string>> DeleteOldTokens(Db db, TimeSpan timeSpan) {
        var time = DateTime.UtcNow.Add(timeSpan);

        var result = await db.SafeExecuteAsync(@"
DELETE 
FROM WebLoginMathResult
WHERE CreateTime <= @Date 
", new { Date = time });
        
        return result;
    }
}