using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Extension;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Dto;

namespace OsuDroidLib.Query;

public static class QueryPlayScore {
    public static async Task<Result<List<PlayScoreWithUsername>, string>> GetBeatmapTop(NpgsqlConnection db,
        string filename, string fileHash) {
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

    public static async Task<Result<Option<PlayScore>, string>> GetUserTopScoreAsync(
        NpgsqlConnection db, long userId, string filename, string fileHash) {
        var sql = @"SELECT * FROM PlayScore WHERE UserId = @UserId AND Filename = @Filename AND Hash = @Hash";
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(sql, new {
            UserId = userId,
            Filename = filename,
            Hash = fileHash
        });
    }

    public static async Task<Result<Option<PlayScore>, string>> GetByIdAsync(NpgsqlConnection db, long playId) {
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


    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreByIdAsync(NpgsqlConnection db, long id) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * FROM PlayScore WHERE PlayScoreId = {id}
");
    }

    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreByIdAndUserIdAsync(
        NpgsqlConnection db, long id, long userId) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * FROM PlayScore WHERE PlayScoreId = {id} AND UserId = {userId}
");
    }

    public static async Task<Result<Option<PlayScore>, string>> GetPlayScoreOldesByUserIdAndHashAsync(
        NpgsqlConnection db, long userId, string mapHash) {
        return await db.SafeQueryFirstOrDefaultAsync<PlayScore>(@$"
SELECT * 
FROM PlayScore 
WHERE UserId = {userId}
AND hash = {mapHash}
ORDER BY date DESC 
LIMIT 1
");
    }

    public static async Task<ResultErr<string>> InsertBblScoreAsync(NpgsqlConnection db, PlayScore newPlayScoreInsert) {
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

    public class MapTopPlays {
        public long PlayScoreId { get; set; }
        public long UserId { get; set; }
        public string? Mode { get; set; }
        public long Score { get; set; }
        public long Combo { get; set; }
        public string? Mark { get; set; }
        public DateTime? Date { get; set; }
        public long Accuracy { get; set; }
        public string? Username { get; set; }
        public long PlayRank { get; set; }
    }

    public static async Task<Result<IReadOnlyList<MapTopPlays>, string>> MapTopPlaysByFilenameAndHashAsync(
        NpgsqlConnection db, string filename, string fileHash, int maxResult) {
        var sql = @$"
SELECT DISTINCT ON (UserId) 
    x.PlayScoreId as PlayScoreId, 
    UserId as UserId, 
    Mode as Mode, 
    Score as Score, 
    Combo as Combo, 
    Mark as Mark, 
    Date as Date, 
    Accuracy as Accuracy, 
    x.Username as Username
FROM (SELECT 
          PlayScore.PlayScoreId, 
          UserId,
          Mode, 
          Score, 
          Combo, 
          Mark, 
          Date, 
          Accuracy, 
          ur.Username
      FROM PlayScore
          FULL JOIN UserInfo ur on PlayScore.UserId = ur.UserId
      WHERE Filename = Filename
        AND Hash = Hash
      ORDER BY PlayScore.Score DESC, PlayScore.Accuracy DESC, PlayScore.Date DESC 
      ) x
LIMIT {maxResult}
";
        var result = (await db.SafeQueryAsync<MapTopPlays>(
            sql,
            new { Filename = filename, Hash = fileHash }
        )).Map(x => x.ToList());

        if (result == EResult.Err)
            return result.ChangeOkType<IReadOnlyList<MapTopPlays>>();

        var plays = result.Ok();

        for (var i = 0; i < plays.Count; i++) {
            plays[i].PlayRank = i + 1;
        }

        return Result<IReadOnlyList<MapTopPlays>, string>.Ok(plays);
    }

    public static async Task<Result<List<PlayScore>, string>> GetTopScoreFromUserIdAsync(NpgsqlConnection db,
        long userId) {
        return (await db.SafeQueryAsync<PlayScore>(@$"
SELECT * 
FROM (
         SELECT distinct ON (filename) * FROM PlayScore
         WHERE Userid = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score DESC 
LIMIT 50;
")).Map(x => x.ToList());
    }

    public static async Task<Result<List<PlayScore>, string>> GetLastPlayScoreFilterByUserIdAsync(
        NpgsqlConnection db, long userId, int limit) {
        var sql = @$"
SELECT * 
FROM PLayScore
WHERE UserId = {userId}
ORDER BY PLayScore.PlayScoreId DESC
LIMIT {limit};
";
        return (await db.SafeQueryAsync<PlayScore>(sql)).Map(x => x.ToList());
    }

    public static async Task<Result<List<PlayScore>, string>> GetTopScoreFromUserIdFilterMark(
        NpgsqlConnection db, long userId, long page, int pageSize, PlayScoreDto.EPlayScoreMark mark) {
        var sql = @$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM PLayScore
         WHERE UserId = {userId}
         AND mark = @Mark
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;";
        return (await db.SafeQueryAsync<PlayScore>(sql, new { Mark = mark.ToStringFast() }))
            .Map(x => x.ToList());
    }

    public static async Task<Result<List<PlayScore>, string>> GetTopScoreFromUserIdWithPageAsync(
        NpgsqlConnection db, long userId, long page, int pageSize) {
        var sql = @$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM PlayScore
         WHERE UserId = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;";

        return (await db.SafeQueryAsync<PlayScore>(sql)).Map(x => x.ToList());
    }

    public static async Task<Result<Dictionary<PlayScoreDto.EPlayScoreMark, long>, string>> CountMarkPlaysByUserIdAsync(
        NpgsqlConnection db, long userId) {
        var sql = @$"
SELECT count(*) as Count, Mark as Mark
FROM PlayScore
WHERE UserId = {userId}
GROUP BY Mark
;";
        var fetchMarkResult = await db.SafeQueryAsync<CountMarkPlaysByUserIdClass>(sql);
        if (fetchMarkResult == EResult.Err)
            return fetchMarkResult.ChangeOkType<Dictionary<PlayScoreDto.EPlayScoreMark, long>>();

        var res = new Dictionary<PlayScoreDto.EPlayScoreMark, long>(12);

        foreach (var row in fetchMarkResult.Ok()) {
            if (Enum.TryParse<PlayScoreDto.EPlayScoreMark>(row.Mark, out _) == false)
                return Result<Dictionary<PlayScoreDto.EPlayScoreMark, long>, string>
                    .Err(TraceMsg.WithMessage($"String ({row.Mark}) Parse To Entities.BblScore.EMark"));
            res[row.GetMarkAsEMark()] = row.Count;
        }

        return Result<Dictionary<PlayScoreDto.EPlayScoreMark, long>, string>.Ok(res);
    }

    private class CountMarkPlaysByUserIdClass {
        public long Count { get; set; }
        public string? Mark { get; set; }

        public PlayScoreDto.EPlayScoreMark GetMarkAsEMark() {
            return EPlayScoreMarkExtensions.TryParse(Mark ?? "", out var found)
                ? found
                : throw new Exception("EPlayScoreMark Not Found");
        }
    }
}