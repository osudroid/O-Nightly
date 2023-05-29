namespace OsuDroid.Database.TableFn; 

public static class BblScoreBanned {
    public static ResultErr<string> DeleteAllScoresBannedByUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_score_banned WHERE uid = {userId}");
    }
}