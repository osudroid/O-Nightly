namespace OsuDroid.Database.TableFn; 

public static class BblScorePreSubmit {
    public static Response DeleteAllScoresPreSubmitByUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_score_pre_submit WHERE uid = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}