namespace OsuDroid.Database.TableFn; 

public static class BblUserStats {
    public static Response DeleteUserStatsByUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_user_stats WHERE uid = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}