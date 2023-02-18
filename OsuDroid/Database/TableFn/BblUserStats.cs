namespace OsuDroid.Database.TableFn; 

public static class BblUserStats {
    public static ResultErr<string> DeleteUserStatsByUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_user_stats WHERE uid = {userId}");
    }
}