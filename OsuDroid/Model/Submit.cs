using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class Submit {
    public static Response<(BblUserStats userStats, long BestPlayScoreId), string> InsertFinishPlayAndUpdateUserScore(
        long userId, ScoreProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var history = BblScorePreSubmit.GetById(db, prop.Id);
        if (history is null)
            return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Err("Map Not Found");

        var newScoreInsert = new BblScore {
            Id = history.Id,
            Uid = history.Uid,
            Filename = history.Filename,
            Hash = history.Hash,
            Mode = prop.Mode,
            Score = prop.Score,
            Combo = prop.Combo,
            Mark = prop.Mark,
            Geki = prop.Geki,
            Perfect = prop.Perfect,
            Katu = prop.Katu,
            Good = prop.Good,
            Bad = prop.Bad,
            Miss = prop.Miss,
            Date = DateTime.UtcNow,
            Accuracy = prop.Accuracy
        };

        var userStats = SqlFunc.GetBblUserStatsByUserId(db, userId).OkOrDefault();
        if (userStats is null)
            return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Err("Id Error");

        if (newScoreInsert.Mode == "AR")
            return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Err("FAIL");

        if (SqlFunc.InsertBblScore(db, newScoreInsert) == EResponse.Err)
            return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Err("FAIL");

        db.Delete<BblScorePreSubmit>(newScoreInsert.Id);


        var userTopScore = BblScore.GetUserTopScore(db, history.Uid, history.Filename!, history.Hash!);

        if (userTopScore is not null) {
            if (userTopScore.Score > newScoreInsert.Score)
                return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Ok((
                    userStats,
                    userTopScore.Id
                ));

            BblUserStats.UpdateStatsFromScore(db, newScoreInsert.Uid, newScoreInsert, userTopScore);

            return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Ok((
                SqlFunc.GetBblUserStatsByUserId(db, userId).Ok(),
                newScoreInsert.Id
            ));
        }

        BblUserStats.UpdateStatsFromScore(db, newScoreInsert.Uid, newScoreInsert);

        return Response<(BblUserStats userStats, long BestPlayScoreId), string>.Ok((
            SqlFunc.GetBblUserStatsByUserId(db, userId).Ok(),
            newScoreInsert.Id
        ));
    }

    public static Response<long, string> InsertPreBuildPlay(long userId, string filename, string fileHash) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var idBblScorePreSubmit = BblScorePreSubmit
            .PreAddScore(
                db,
                userId,
                filename,
                fileHash);

        if (idBblScorePreSubmit is null)
            return Response<long, string>.Err("Server Error");
        return Response<long, string>.Ok(idBblScorePreSubmit.Id);
    }

    public class ScoreProp {
        public string? Mode { get; set; }
        public string? Mark { get; set; }
        public long Id { get; set; }
        public long Score { get; set; }
        public long Combo { get; set; }
        public long Uid { get; set; }
        public long Geki { get; set; }
        public long Perfect { get; set; }
        public long Katu { get; set; }
        public long Good { get; set; }
        public long Bad { get; set; }
        public long Miss { get; set; }
        public long Accuracy { get; set; }
    }
}