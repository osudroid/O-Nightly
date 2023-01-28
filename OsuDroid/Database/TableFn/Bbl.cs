using OsuDroidLib.Database.Entities;

namespace OsuDroid.Database.TableFn; 

public static class Bbl {
    public static Response DeleteAccount(SavePoco db, long userId) {
        var bblUser = BblUser.GetUserById(db, userId);
        if (bblUser is null)
            return Response.Err();
        return BblAvatarHash.DeleteAvatarHashByAllUserId(db, userId) == EResponse.Err
               || BblGlobalRankingTimeline.DeleteAllRankingByAllUserId(db, userId) == EResponse.Err
               || BblScore.DeleteAllScoresByUserId(db, userId) == EResponse.Err
               || BblScoreBanned.DeleteAllScoresBannedByUserId(db, userId) == EResponse.Err
               || BblScorePreSubmit.DeleteAllScoresPreSubmitByUserId(db, userId) == EResponse.Err
               || BblUserStats.DeleteUserStatsByUserId(db, userId) == EResponse.Err
               || BblUser.DeleteBblUser(db, userId) == EResponse.Err
            ? Response.Err()
            : Response.Ok();
    }
}