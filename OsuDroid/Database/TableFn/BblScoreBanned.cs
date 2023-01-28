namespace OsuDroid.Database.TableFn; 

public static class BblScoreBanned {
    public static Response DeleteAllScoresBannedByUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_score_banned WHERE uid = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}