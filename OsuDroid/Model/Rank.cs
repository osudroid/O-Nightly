using NPoco;

namespace OsuDroid.Model;

public class MapTopPlays {
    [Column("play_id")] public long PlayId { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("mode")] public string? Mode { get; set; }
    [Column("score")] public long Score { get; set; }
    [Column("combo")] public long Combo { get; set; }
    [Column("mark")] public string? Mark { get; set; }
    [Column("date")] public DateTime? Date { get; set; }
    [Column("accuracy")] public long Accuracy { get; set; }
    [Column("username")] public string? Username { get; set; }
    public long PlayRank { get; set; }
}

public static class Rank {
    public static Result<IReadOnlyList<MapTopPlays>, string> MapTopPlaysByFilenameAndHash(
        string filename, string fileHash, int maxResult) {
        
        var sql = new Sql(@$"
SELECT DISTINCT ON (uid) 
    x.id as play_id, 
    uid as user_id, 
    mode as mode, 
    score as score, 
    combo as combo, 
    mark as mark, 
    date as date, 
    accuracy as accuracy, 
    x.username as username
FROM (SELECT 
          bbl_score.id, 
          uid,
          mode, 
          score, 
          combo, 
          mark, 
          date, 
          accuracy, 
          ur.username
      FROM bbl_score
          FULL JOIN bbl_user ur on bbl_score.uid = ur.id
      WHERE filename = @1
        AND hash = @2
      ORDER BY bbl_score.score DESC, bbl_score.accuracy DESC, bbl_score.date DESC 
      ) x
LIMIT {maxResult}", filename, fileHash);

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var plays = db.Fetch<MapTopPlays>(sql).OkOr(new List<MapTopPlays>());

        for (var i = 0; i < plays.Count; i++) {
            Console.WriteLine($"i={i}");
            plays[i].PlayRank = i + 1;
        }
        Console.WriteLine("End");
        return Result<IReadOnlyList<MapTopPlays>, string>.Ok(plays);
    }
}