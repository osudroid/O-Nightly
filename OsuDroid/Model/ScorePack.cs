using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class ScorePack {
    public static Response<(BblScore Score, string Username, string Region)> GetByPlayId(long playId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var score = db.FirstOrDefault<BblScore>($"SELECT * FROM bbl_user WHERE id = {playId} LIMIT 1").OkOrDefault();

        if (score is null)
            return Response<(BblScore Score, string Username, string Region)>.Err;

        var user = db.FirstOrDefault<BblUser>(@$"
SELECT username, region
FROM bbl_user
WHERE id = {score.Uid}
").OkOrDefault();
        if (user is null)
            return Response<(BblScore Score, string Username, string Region)>.Err;
        return Response<(BblScore Score, string Username, string Region)>
            .Ok((score, user.Username!, user.Region!));
    }
}