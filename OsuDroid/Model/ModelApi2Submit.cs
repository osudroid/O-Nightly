using Npgsql;
using OsuDroid.Class;
using OsuDroid.View;
using OsuDroid.Extensions;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class ModelApi2Submit {
    public static async Task<Result<ModelResult<ViewPushReplayResult200>, string>>
        InsertFinishPlayAndUpdateUserScoreAsync(NpgsqlConnection db, long userId, ScoreProp prop) {
        var resultHistory = await PlayScorePreSubmitHandler.GetByIdAsync(db, prop.Id);
        if (resultHistory == EResult.Err)
            return resultHistory.ChangeOkType<ModelResult<ViewPushReplayResult200>>();

        var optionHistory = resultHistory.Ok();
        if (optionHistory.IsSet() == false)
            return Result<ModelResult<ViewPushReplayResult200>, string>.Ok(ModelResult<ViewPushReplayResult200>
                .BadRequest());

        var history = optionHistory.Unwrap();

        string[]? fixMode;
        try {
            fixMode = Mode.ModeAsSingleStringToModeArray(prop.Mode);
        }
        catch (Exception e) {
            return Result<ModelResult<ViewPushReplayResult200>, string>.Err(e.ToString());
        }

        PlayScore newScoreInsert = new PlayScore {
            PlayScoreId = history.PlayScoreId,
            UserId = history.UserId,
            Filename = history.Filename,
            Hash = history.Hash,
            Mode = fixMode,
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


        if ((newScoreInsert.Mode ?? Array.Empty<string>()).Contains("AR"))
            return Result<ModelResult<ViewPushReplayResult200>, string>
                .Err(TraceMsg.WithMessage("FAIL AR In Mode"));


        var resultErrInsert = await QueryPlayScore.InsertBblScoreAsync(db, newScoreInsert);
        if (resultErrInsert == EResult.Err)
            return Result<ModelResult<ViewPushReplayResult200>, string>.Err(resultErrInsert.Err());

        var resultErrDb = await QueryPlayScorePreSubmit.DeleteByIdAsync(db, newScoreInsert.PlayScoreId);
        if (resultErrDb == EResult.Err)
            return Result<ModelResult<ViewPushReplayResult200>, string>.Err(resultErrDb.Err());

        var resultUserTopScore = await QueryPlayScore.GetUserTopScoreAsync(
            db, history.UserId, history.Filename!, history.Hash!);

        if (resultUserTopScore == EResult.Err)
            return Result<ModelResult<ViewPushReplayResult200>, string>.Err(resultUserTopScore.Err());

        var optionUserTopScore = resultUserTopScore.Ok();

        if (optionUserTopScore.IsSet() == false)
            return Result<ModelResult<ViewPushReplayResult200>, string>.Ok(ModelResult<ViewPushReplayResult200>
                .BadRequest());

        var newScoreInsertDto = OsuDroidLib.Dto.PlayScoreDto.ToPlayScoreDto(newScoreInsert).Unwrap();

        if (optionUserTopScore.IsSet()) {
            var userTopScore = optionUserTopScore.Unwrap();

            if (userTopScore.Score > newScoreInsert.Score)
                return Result<ModelResult<ViewPushReplayResult200>, string>
                    .Ok(ModelResult<ViewPushReplayResult200>.Ok(new ViewPushReplayResult200() {
                        BestPlayScoreId = userTopScore.UserId,
                        UserStats = ViewUserStats.FromUserStats(userStats.Unwrap())
                    }));


            var result = await QueryUserStats.UpdateStatsFromScoreAsync(
                db,
                newScoreInsert.UserId,
                newScoreInsertDto,
                OsuDroidLib.Dto.PlayScoreDto.ToPlayScoreDto(userTopScore).Unwrap()
            );

            if (result == EResult.Err)
                return Result<ModelResult<ViewPushReplayResult200>, string>
                    .Err(result.Err());

            return Result<ModelResult<ViewPushReplayResult200>, string>
                .Ok(ModelResult<ViewPushReplayResult200>.Ok(new ViewPushReplayResult200() {
                    BestPlayScoreId = newScoreInsert.PlayScoreId,
                    UserStats = ViewUserStats.FromUserStats((await QueryUserStats
                        .GetBblUserStatsByUserIdAsync(db, userId)).Ok().Unwrap())
                }));
        }

        var resultErr = await QueryUserStats.UpdateStatsFromScoreAsync(db, newScoreInsert.UserId, newScoreInsertDto);

        if (resultErr == EResult.Err)
            return Result<ModelResult<ViewPushReplayResult200>, string>
                .Err(resultErr.Err());

        return Result<ModelResult<ViewPushReplayResult200>, string>
            .Ok(ModelResult<ViewPushReplayResult200>
                .Ok(new ViewPushReplayResult200() {
                    BestPlayScoreId = newScoreInsert.UserId,
                    UserStats = ViewUserStats.FromUserStats(
                        (await QueryUserStats.GetBblUserStatsByUserIdAsync(db, userId)).Ok().Unwrap())
                }));
    }

    public static async Task<Result<ModelResult<long>, string>> InsertPreBuildPlayAsync(
        NpgsqlConnection db, long userId, string filename, string fileHash) {
        var idBblScorePreSubmit = await QueryPlayScorePreSubmit
            .PreAddScoreAsync(
                db,
                userId,
                filename,
                fileHash);

        if (idBblScorePreSubmit == EResult.Err)
            return idBblScorePreSubmit.ChangeOkType<ModelResult<long>>();

        return Result<ModelResult<long>, string>.Ok(ModelResult<long>.Ok(idBblScorePreSubmit.Ok().PlayScoreId));
    }


    public interface IScoreProp {
        public long Id { get; }
        public long Uid { get; }
        public string? Filename { get; }
        public string? Hash { get; }
        public string? Mode { get; }
        public long Score { get; }
        public long Combo { get; }
        public string? Mark { get; }
        public long Geki { get; }
        public long Perfect { get; }
        public long Katu { get; }
        public long Good { get; }
        public long Bad { get; }
        public long Miss { get; }
        public long Accuracy { get; }
    }

    public class ScoreProp : IScoreProp {
        public long Id { get; set; }
        public long Uid { get; set; }
        public string? Filename { get; set; }
        public string? Hash { get; set; }
        public string? Mode { get; set; }
        public long Score { get; set; }
        public long Combo { get; set; }
        public string? Mark { get; set; }
        public long Geki { get; set; }
        public long Perfect { get; set; }
        public long Katu { get; set; }
        public long Good { get; set; }
        public long Bad { get; set; }
        public long Miss { get; set; }
        public long Accuracy { get; set; }
    }
}