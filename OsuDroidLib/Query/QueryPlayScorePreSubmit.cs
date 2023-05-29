using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryPlayScorePreSubmit {
    /// <summary> INSERT NEW ROW RETURN WITH Id </summary>
    /// <param name="db"></param>
    /// <param name="userId"></param>
    /// <param name="filename"></param>
    /// <param name="fileHash"></param>
    /// <returns></returns>
    public static async Task<Result<PlayScorePreSubmit, string>> PreAddScore(NpgsqlConnection db, long userId, string filename, string fileHash) {
        var insert = new PlayScorePreSubmit {
            UserId = userId,
            Filename = filename,
            Hash = fileHash,
            Date = DateTime.UtcNow,
            Mode = Array.Empty<string>(),
            Mark = string.Empty
        };
        var result = await db.SafeInsertAsync(insert);
        if (result == EResult.Err)
            return result.ChangeOkType<PlayScorePreSubmit>();

        return (await db.SafeQuerySingleAsync<BoxLong>(@"
SELECT ScorePreSubmitId as Value 
FROM PlayScorePreSubmit 
WHERE UserId = @UserId
  AND Filename = @Filename
  AND Hash = @FileHash

", new { UserId = userId, Filename = filename, FileHash = fileHash }))
            .Map(x => {
                insert.PlayScoreId = x.Value;
                return insert;
            });
    }

    public static async Task<Result<Option<PlayScorePreSubmit>, string>> GetById(NpgsqlConnection db, long id) {
        return (await db.SafeQueryFirstOrDefaultAsync<PlayScorePreSubmit>(@$"
SELECT * FROM PlayScorePreSubmit WHERE ScorePreSubmitId = @Id
", new { Id = id }));
    }
}