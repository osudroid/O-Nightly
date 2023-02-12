namespace OsuDroid.Database.TableFn; 

public class BblAvatarHash {
    public static ResultErr<string> DeleteAvatarHashByAllUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_avatar_hash WHERE user_id = {userId}");
    }
}