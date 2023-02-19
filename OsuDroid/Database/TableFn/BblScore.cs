using NPoco;

namespace OsuDroid.Database.TableFn; 

public static class BblScore {
    public static ResultErr<string> DeleteAllScoresByUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_score WHERE uid = {userId}");
    }
    
    public static Result<List<Entities.BblScore>, string> GetTopScoreFromUserId(SavePoco db, long userId) {
        return db.Fetch<Entities.BblScore>(@$"
SELECT * 
FROM (
         SELECT distinct ON (filename) * FROM bbl_score
         WHERE uid = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score DESC 
LIMIT 50;
");
    }

    public static Result<Dictionary<Entities.BblScore.EMark, long>, string> CountMarkPlaysByUserId(SavePoco db, long userId) {
        var sql = new Sql(@$"
SELECT count(*) as count, mark as mark
FROM bbl_score
WHERE uid = {userId}
GROUP BY osu_droid.public.bbl_score.mark
;");
        var fetchMarkResult = db.Fetch<CountMarkPlaysByUserIdClass>(sql);
        if (fetchMarkResult == EResult.Err)
            return Result<Dictionary<Entities.BblScore.EMark, long>, string>.Err(fetchMarkResult.Err());
        
        var res = new Dictionary<Entities.BblScore.EMark, long>(8);
        
        foreach (var row in fetchMarkResult.Ok()) {
            res[row.Mark] = row.Count;
        }

        return Result<Dictionary<Entities.BblScore.EMark, long>, string>.Ok(res);
    }

    public static Result<List<Entities.BblScore>, string> GetTopScoreFromUserIdFilterMark(
        SavePoco db, long userId, long page, int pageSize, Entities.BblScore.EMark mark) {

        var sql = new Sql(@$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM bbl_score
         WHERE uid = {userId}
         AND mark = @0
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;", mark);

        return db.Fetch<Entities.BblScore>(sql);
    }
    
    public static Result<List<Entities.BblScore>, string> GetTopScoreFromUserIdWithPage(
        SavePoco db, long userId, long page, int pageSize) {

        var sql = new Sql(@$"
SELECT *
FROM (
         SELECT distinct ON (filename) * FROM bbl_score
         WHERE uid = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score
LIMIT {pageSize}
OFFSET {page * pageSize}
;");

        return db.Fetch<Entities.BblScore>(sql);
    }
    
    private class CountMarkPlaysByUserIdClass {
        [Column("count")] public long Count { get; set; }
        [Column("mark")] public Entities.BblScore.EMark Mark { get; set; }
    }
}