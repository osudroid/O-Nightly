using Npgsql;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class ScorePack {
    public static async Task<Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>>
        GetByPlayIdAsync(
            NpgsqlConnection db,
            long playId) {
        var resultScore = await QueryPlayScore.GetPlayScoreByIdAsync(db, playId);

        if (resultScore == EResult.Err)
            Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>.Err(resultScore.Err());

        var optionScore = resultScore.Ok();

        if (optionScore.IsSet() == false)
            return Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>
                .Ok(Option<(Entities.PlayScore Score, string Username, string Region)>.Empty);


        var score = optionScore.Unwrap();


        var resultUser = await QueryUserInfo.GetUsernameAndRegionByUserId(db, score.UserId);
        if (resultUser == EResult.Err)
            return Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>.Err(
                resultUser.Err()
            );

        if (resultUser.Ok().IsSet() == false)
            return Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>
                .Ok(Option<(Entities.PlayScore Score, string Username, string Region)>.Empty);

        var user = resultUser.Ok().Unwrap();
        return Result<Option<(Entities.PlayScore Score, string Username, string Region)>, string>
            .Ok(Option<(Entities.PlayScore Score, string Username, string Region)>.With((score, user.Username!,
                        user.Region!)
                )
            );
    }
}