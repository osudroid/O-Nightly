using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class Submit {
    public static Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string> InsertFinishPlayAndUpdateUserScore(
        long userId, ScoreProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var resultHistory = BblScorePreSubmit.GetById(db, prop.Id);
        if (resultHistory == EResult.Err)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err(resultHistory.Err());

        var optionHistory = resultHistory.Ok();
        if (optionHistory.IsSet() == false)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(BblUserStats userStats, long BestPlayScoreId)>.Empty);

        var history = optionHistory.Unwrap();
        
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

        Result<BblUserStats, string> resultUserStats = SqlFunc.GetBblUserStatsByUserId(db, userId);
        if (resultUserStats == EResult.Err)
            Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err(resultUserStats.Err());
        
        var userStats = resultUserStats.Ok();
        
        if (newScoreInsert.Mode == "AR")
            Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err("FAIL");

        var resultErrInsert = SqlFunc.InsertBblScore(db, newScoreInsert);
        if (resultErrInsert == EResult.Err)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err(resultErrInsert.Err());

        var resultErrDb = db.Delete<BblScorePreSubmit>(newScoreInsert.Id);
        if (resultErrDb == EResult.Err)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err(resultErrDb.Err());

        var resultUserTopScore = BblScore.GetUserTopScore(db, history.Uid, history.Filename!, history.Hash!);

        if (resultUserTopScore == EResult.Err)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>.Err(resultUserTopScore.Err());

        var optionUserTopScore = resultUserTopScore.Ok();
        
        if (optionUserTopScore.IsSet() == false)
            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(BblUserStats userStats, long BestPlayScoreId)>.Empty);
        
        if (optionUserTopScore.IsSet()) {
            var userTopScore = optionUserTopScore.Unwrap();
            
            if (userTopScore.Score > newScoreInsert.Score)
                return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>
                    .Ok(Option<(BblUserStats userStats, long BestPlayScoreId)>
                        .With((
                            userStats,
                            userTopScore.Id
                        )));

            BblUserStats.UpdateStatsFromScore(db, newScoreInsert.Uid, newScoreInsert, userTopScore);

            return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(BblUserStats userStats, long BestPlayScoreId)>
                    .With((
                        SqlFunc.GetBblUserStatsByUserId(db, userId).Ok(),
                        newScoreInsert.Id
                    )));
        }

        BblUserStats.UpdateStatsFromScore(db, newScoreInsert.Uid, newScoreInsert);

        return Result<Option<(BblUserStats userStats, long BestPlayScoreId)>, string>
            .Ok(Option<(BblUserStats userStats, long BestPlayScoreId)>
                .With((
                    SqlFunc.GetBblUserStatsByUserId(db, userId).Ok(),
                    newScoreInsert.Id
                )));
    }

    public static Result<long, string> InsertPreBuildPlay(long userId, string filename, string fileHash) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var idBblScorePreSubmit = BblScorePreSubmit
            .PreAddScore(
                db,
                userId,
                filename,
                fileHash);

        if (idBblScorePreSubmit is null)
            return Result<long, string>.Err("Server Error");
        return Result<long, string>.Ok(idBblScorePreSubmit.Id);
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