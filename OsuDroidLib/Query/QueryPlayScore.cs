using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Extension;
using OsuDroidLib.Database.Entities;

namespace OsuDroidLib.Query; 

public static class QueryPlayScore {
    public static async Task<Result<List<PlayScoreWithUsername>, string>> GetBeatmapTop(NpgsqlConnection db, string filename, string fileHash) {
        var sql = "SELECT beatmap_top(@File, @FileHash, @LimitCount)";

        return (await db.SafeQueryAsync<PlayScoreWithUsername>(sql, new {
            File = filename,
            FileHash = fileHash,
            LimitCount = 50
        })).Map(x => {
            return x.Select(x => {
                x.Username ??= string.Empty;
                x.Filename ??= string.Empty;
                x.Hash ??= string.Empty;
                return x;
            });
        }).Map(x => x.ToList());
    }

    public static async Task<Result<Option<PlayScore>, string>> GetUserTopScore(
        NpgsqlConnection db, long userId, string filename, string fileHash) {
        
        var sql = @"SELECT * FROM PlayScore WHERE UserId = @UserId AND Filename = @Filename AND Hash = @Hash";
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(sql, new {
            UserId = userId,
            Filename = filename,
            Hash = fileHash
        });
    }

    public static async Task<Result<Option<PlayScore>, string>> GetById(NpgsqlConnection db, long playId) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(
            $"SELECT * FROM PlayScore WHERE UserId = {playId} LIMIT 1"
            );
    }


    public static async Task<Result<long, string>> GetUserMapRank(NpgsqlConnection db, long playId) {
        return (await db.SafeQueryFirstAsync<BoxLong>(@$"
SELECT count(*) + 1 as Value 
FROM PlayScore 
    TotalScore, (SELECT filename, hash, score FROM PlayScore WHERE PlayScoreId = {playId}) refer 
WHERE TotalScore.Filename = refer.Filename 
  AND TotalScore.Hash = refer.Hash 
  AND TotalScore.Score > refer.Score
")).Map(x => x.Value);
    }
    
    
    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreById(NpgsqlConnection db, long id) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * FROM PlayScore WHERE PlayScoreId = {id}
");
    }

    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreByIdAndUserId(NpgsqlConnection db, long id, long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * FROM PlayScore WHERE PlayScoreId = {id} AND UserId = {userId}
");
    }

    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreOldesByUserIdAndHash(NpgsqlConnection db, long userId, string mapHash) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * 
FROM PlayScore 
WHERE UserId = {userId}
AND hash = {mapHash}
ORDER BY date DESC 
LIMIT 1
");
    }
    
    public static async Task<ResultErr<string>> InsertBblScore(NpgsqlConnection db, PlayScore newPlayScoreInsert) {
        var sql = @$"
INSERT 
INTO PlayScore (PlayScoreId, UserId, Filename, Hash, Mode, Score, Combo, Mark, Geki, Perfect, Katu, Good, Bad, Miss, Date, Accuracy) 
VALUES
    (
     @PlayScoreId,
     @UserId,
     @Filename,
     @Hash,
     @Mode,
     @Score,
     @Combo,
     @Mark,
     @Geki,
     @Perfect,
     @Katu,
     @Good,
     @Bad,
     @Miss,
     @Date,
     @Accuracy
     "; 
        return await db.SafeQueryAsync(sql, new {
            PlayScoreId = newPlayScoreInsert.PlayScoreId, 
            UserId = newPlayScoreInsert.UserId, 
            Filename = newPlayScoreInsert.Filename, 
            Hash = newPlayScoreInsert.Hash, 
            Mode = newPlayScoreInsert.Mode, 
            Score = newPlayScoreInsert.Score, 
            Combo = newPlayScoreInsert.Combo, 
            Mark = newPlayScoreInsert.Mark, 
            Geki = newPlayScoreInsert.Geki, 
            Perfect = newPlayScoreInsert.Perfect, 
            Katu = newPlayScoreInsert.Katu, 
            Good = newPlayScoreInsert.Good, 
            Bad = newPlayScoreInsert.Bad, 
            Miss = newPlayScoreInsert.Miss, 
            Date = newPlayScoreInsert.Date, 
            Accuracy = newPlayScoreInsert.Accuracy
        });
    }
}