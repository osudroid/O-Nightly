namespace OsuDroid.Database.TableFn; 

public static class BblScore {
    public static Response DeleteAllScoresByUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_score WHERE uid = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}