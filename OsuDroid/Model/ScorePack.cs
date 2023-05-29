using System.Xml.Linq;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class ScorePack {
    public static Result<Option<(PlayScore Score, string Username, string Region)>, string> GetByPlayId(SavePoco db, long playId) {
        var resultScore = db.FirstOrDefault<PlayScore>($"SELECT * FROM bbl_score WHERE id = {playId} LIMIT 1")
            .Map(x => Option<PlayScore>.NullSplit(x));

        if (resultScore == EResult.Err)
            Result<Option<(PlayScore Score, string Username, string Region)>, string>.Err(resultScore.Err());

        var optionScore = resultScore.Ok();
        
        if (optionScore.IsSet() == false)
            return Result<Option<(PlayScore Score, string Username, string Region)>, string>
                .Ok(Option<(PlayScore Score, string Username, string Region)>.Empty);

        
        var score = optionScore.Unwrap();
        var resultUser = db.FirstOrDefault<UserInfo>(@$"
SELECT username, region
FROM bbl_user
WHERE id = {score.Uid}
").Map(x => Option<UserInfo>.NullSplit(x));
        if (resultUser == EResult.Err)
            return Result<Option<(PlayScore Score, string Username, string Region)>, string>.Err(resultUser.Err());
        
        if (resultUser.Ok().IsSet() == false)
            return Result<Option<(PlayScore Score, string Username, string Region)>, string>
                .Ok(Option<(PlayScore Score, string Username, string Region)>.Empty);

        var user = resultUser.Ok().Unwrap();
        return Result<Option<(PlayScore Score, string Username, string Region)>, string>
            .Ok(Option<(PlayScore Score, string Username, string Region)>.With((score, user.Username!, user.Region!)));
    }
}