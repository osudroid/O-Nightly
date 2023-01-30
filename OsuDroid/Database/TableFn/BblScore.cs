namespace OsuDroid.Database.TableFn; 

public static class BblScore {
    public static Response DeleteAllScoresByUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_score WHERE uid = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
    
    public static Response<List<Entities.BblScore>, string> GetTopScoreFromUserId(SavePoco db, long userId) {
        var res = db.Fetch<Entities.BblScore>(@$"
SELECT * 
FROM (
         SELECT distinct ON (filename) * FROM bbl_score
         WHERE uid = {userId}
         ORDER BY filename, score DESC
     ) x
ORDER BY score DESC 
LIMIT 50;
");
        if (res == EResponse.Err)
            return Response<List<Entities.BblScore>, string>.Err("DB Error");
        return Response<List<Entities.BblScore>, string>.Ok(res.Ok());
    }
}