namespace OsuDroid.Database.TableFn; 

public class BblAvatarHash {
    public static Response DeleteAvatarHashByAllUserId(SavePoco db, long userId) {
        var res = db.Execute(@$"Delete FROM bbl_avatar_hash WHERE user_id = {userId}");
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}