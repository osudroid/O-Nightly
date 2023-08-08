using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class WebProfileStatsHandler 
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<UserIdBoxDto>, OptionHandlerOutput<ViewProfileStats>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewProfileStats>, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerGetWrapper<UserIdBoxDto> request) {
        var db = dbWrapper.Db;
        var userId = request.Get.UserId;
        var log = logger.Logger;
        var controller = request.Controller;
        
        var optionUserAndStatsResult = await Query.GetUserInfoAndBblUserStatsByUserIdAsync(db, userId);

        if (optionUserAndStatsResult == EResult.Err)
            return optionUserAndStatsResult.ChangeOkType<OptionHandlerOutput<ViewProfileStats>>();

        if (optionUserAndStatsResult.Ok().IsNotSet()) {
            return Result<OptionHandlerOutput<ViewProfileStats>, string>
                .Ok(OptionHandlerOutput<ViewProfileStats>.With(new ViewProfileStats() {
                    Found = false
                }));
        }

        var userInfoAndStats = optionUserAndStatsResult.Ok().Unwrap();
        var resultRankOpt = await Query.GetUserRankAsync(db, userId, userInfoAndStats.OverallScore);
        if (resultRankOpt == EResult.Err)
            return resultRankOpt.ChangeOkType<OptionHandlerOutput<ViewProfileStats>>();

        var rankOpt = resultRankOpt.Ok();
        if (rankOpt.IsNotSet())
            return Result<OptionHandlerOutput<ViewProfileStats>, string>.Err("Error Not Found");


        Option<Entities.Patron> optionBblPatron = Option<Entities.Patron>.Empty;
        if ((userInfoAndStats.Email ?? "").Length == 0) {
            optionBblPatron = (await log.AddResultAndTransformAsync(await QueryPatron
                    .GetByPatronEmailAsync(db, userInfoAndStats.Email ?? "")))
                .OkOrDefault();
        }

        return Result<OptionHandlerOutput<ViewProfileStats>, string>.Ok(OptionHandlerOutput<ViewProfileStats>.With(new ViewProfileStats() {
            Username = userInfoAndStats.Username,
            Id = userInfoAndStats.UserId,
            Found = true,
            OverallPlaycount = userInfoAndStats.OverallPlaycount,
            Region = userInfoAndStats.Region,
            Active = userInfoAndStats.Active,
            Supporter = optionBblPatron.IsSet() && optionBblPatron.Unwrap().ActiveSupporter,
            GlobalRanking = rankOpt.Unwrap().GlobalRank,
            CountryRanking = rankOpt.Unwrap().CountryRank,
            OverallScore = userInfoAndStats.OverallScore,
            OverallAccuracy = userInfoAndStats.OverallAccuracy,
            OverallCombo = userInfoAndStats.OverallCombo,
            OverallXss = userInfoAndStats.OverallXss,
            OverallSs = userInfoAndStats.OverallSs,
            OverallXs = userInfoAndStats.OverallXs,
            OverallS = userInfoAndStats.OverallS,
            OverallA = userInfoAndStats.OverallA,
            OverallB = userInfoAndStats.OverallB,
            OverallC = userInfoAndStats.OverallC,
            OverallD = userInfoAndStats.OverallD,
            OverallHits = userInfoAndStats.OverallHits,
            OverallPerfect = userInfoAndStats.OverallPlaycount,
            Overall300 = userInfoAndStats.Overall300,
            Overall100 = userInfoAndStats.Overall100,
            Overall50 = userInfoAndStats.Overall50,
            OverallGeki = userInfoAndStats.OverallGeki,
            OverallKatu = userInfoAndStats.OverallKatu,
            OverallMiss = userInfoAndStats.OverallMiss,
            RegistTime = userInfoAndStats.RegisterTime,
            LastLoginTime = userInfoAndStats.LastLoginTime
        }));
    }
}