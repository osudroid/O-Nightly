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
}