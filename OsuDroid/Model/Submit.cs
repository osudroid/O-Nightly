using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class Submit {
    public static async Task<Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>> 
        InsertFinishPlayAndUpdateUserScoreAsync(NpgsqlConnection db, long userId, ScoreProp prop) {

        var resultHistory = await PlayScorePreSubmitHandler.GetByIdAsync(db, prop.PlayScoreId);
        if (resultHistory == EResult.Err)
            return resultHistory.ChangeOkType<Option<(UserStats userStats, long BestPlayScoreId)>>();

        var optionHistory = resultHistory.Ok();
        if (optionHistory.IsSet() == false)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(UserStats userStats, long BestPlayScoreId)>.Empty);

        var history = optionHistory.Unwrap();
        
        PlayScore newScoreInsert = new PlayScore {
            PlayScoreId = history.PlayScoreId,
            UserId = history.UserId,
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

        var resultUserStats = await QueryUserStats.GetBblUserStatsByUserIdAsync(db, userId);
        if (resultUserStats == EResult.Err)
            Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>.Err(resultUserStats.Err());
        
        var userStats = resultUserStats.Ok();
        
        
        if ((newScoreInsert.Mode??Array.Empty<string>()).Contains("AR"))
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                .Err(TraceMsg.WithMessage("FAIL AR In Mode"));

        
        var resultErrInsert = await QueryPlayScore.InsertBblScoreAsync(db, newScoreInsert);
        if (resultErrInsert == EResult.Err)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>.Err(resultErrInsert.Err());

        var resultErrDb = await QueryPlayScorePreSubmit.DeleteByIdAsync(db, newScoreInsert.PlayScoreId);
        if (resultErrDb == EResult.Err)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>.Err(resultErrDb.Err());

        var resultUserTopScore = await QueryPlayScore.GetUserTopScoreAsync(
            db, history.UserId, history.Filename!, history.Hash!);

        if (resultUserTopScore == EResult.Err)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>.Err(resultUserTopScore.Err());

        var optionUserTopScore = resultUserTopScore.Ok();
        
        if (optionUserTopScore.IsSet() == false)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(UserStats userStats, long BestPlayScoreId)>.Empty);
        
        var newScoreInsertDto = OsuDroidLib.Dto.PlayScoreDto.ToPlayScoreDto(newScoreInsert).Unwrap();
        
        if (optionUserTopScore.IsSet()) {
            var userTopScore = optionUserTopScore.Unwrap();
            
            if (userTopScore.Score > newScoreInsert.Score)
                return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                    .Ok(Option<(UserStats userStats, long BestPlayScoreId)>
                        .With((
                            userStats.Unwrap(),
                            userTopScore.UserId
                        )));


            var result = await QueryUserStats.UpdateStatsFromScoreAsync(
                db, 
                newScoreInsert.UserId, 
                newScoreInsertDto, 
                OsuDroidLib.Dto.PlayScoreDto.ToPlayScoreDto(userTopScore).Unwrap()
                );
            
            if (result == EResult.Err)
                return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                    .Err(result.Err());
            
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                .Ok(Option<(UserStats userStats, long BestPlayScoreId)>
                    .With((
                        (await QueryUserStats.GetBblUserStatsByUserIdAsync(db, userId)).Ok().Unwrap(),
                        newScoreInsert.PlayScoreId
                    )));
        }

        var resultErr = await QueryUserStats.UpdateStatsFromScoreAsync(db, newScoreInsert.UserId, newScoreInsertDto);

        if (resultErr == EResult.Err)
            return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
                .Err(resultErr.Err());
        
        return Result<Option<(UserStats userStats, long BestPlayScoreId)>, string>
            .Ok(Option<(UserStats userStats, long BestPlayScoreId)>
                .With((
                    (await QueryUserStats.GetBblUserStatsByUserIdAsync(db, userId)).Ok().Unwrap(),
                    newScoreInsert.UserId
                )));
    }

    public static async Task<Result<long, string>> InsertPreBuildPlayAsync(
        NpgsqlConnection db, long userId, string filename, string fileHash) {
        
        var idBblScorePreSubmit = await QueryPlayScorePreSubmit
            .PreAddScoreAsync(
                db,
                userId,
                filename,
                fileHash);

        if (idBblScorePreSubmit == EResult.Err)
            return idBblScorePreSubmit.ChangeOkType<long>();
        
        return Result<long, string>.Ok(idBblScorePreSubmit.Ok().PlayScoreId);
    }

    public class ScoreProp {
        public long PlayScoreId  { get; set; }
        public long UserId   { get; set; }
        public string? Filename { get; set; }
        public string? Hash     { get; set; }
        public string[]? Mode     { get; set; }
        public long Score    { get; set; }
        public long Combo    { get; set; }
        public string? Mark     { get; set; }
        public long Geki     { get; set; }
        public long Perfect  { get; set; }
        public long Katu     { get; set; }
        public long Good     { get; set; }
        public long Bad      { get; set; }
        public long Miss     { get; set; }
        public long Accuracy { get; set; }
    }
}