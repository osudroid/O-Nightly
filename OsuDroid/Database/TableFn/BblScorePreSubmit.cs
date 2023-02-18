namespace OsuDroid.Database.TableFn; 

public static class BblScorePreSubmit {
    public static ResultErr<string> DeleteAllScoresPreSubmitByUserId(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_score_pre_submit WHERE uid = {userId}");
    }
}