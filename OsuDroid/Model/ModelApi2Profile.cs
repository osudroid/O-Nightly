using LamLogger;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroidLib.Dto;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Model; 

public static class ModelApi2Profile {
    /// <summary> Guid Token Id, long UserId </summary>
    /// <returns></returns>
    private static readonly TimeoutTokenDictionary<Guid, (long UserId, string Email)> _patreoneMailToken =
        new(TimeSpan.FromMinutes(5), 10000, 10000);

    private static readonly TimeoutTokenDictionary<Guid, (long UserId, string Email)> _deleteAccMailToken =
        new(TimeSpan.FromMinutes(5), 10000, 10000);

    public static async Task<Result<ModelResult<ViewProfileStats>, string>> WebProfileStatsAsync(
        ControllerExtensions controller, NpgsqlConnection db, LamLog log, long userId) {
        
        var optionUserAndStatsResult = await Query.GetUserInfoAndBblUserStatsByUserIdAsync(db, userId);
        
        if (optionUserAndStatsResult == EResult.Err)
            return optionUserAndStatsResult.ChangeOkType<ModelResult<ViewProfileStats>>();

        if (optionUserAndStatsResult.Ok().IsNotSet())
            return Result<ModelResult<ViewProfileStats>, string>
                .Ok(ModelResult<ViewProfileStats>.Ok(new ViewProfileStats { Found = false }));

        var userInfoAndStats = optionUserAndStatsResult.Ok().Unwrap();
        var resultRankOpt = await Query.GetUserRankAsync(db, userId, userInfoAndStats.OverallScore);
        if (resultRankOpt == EResult.Err)
            return resultRankOpt.ChangeOkType<ModelResult<ViewProfileStats>>();
        
        var rankOpt = resultRankOpt.Ok();
        if (rankOpt.IsNotSet())
            return Result<ModelResult<ViewProfileStats>, string>.Err("Error Not Found");


        Option<Entities.Patron> optionBblPatron = Option<Entities.Patron>.Empty;
        if ((userInfoAndStats.Email??"").Length == 0) {
            optionBblPatron = (await log.AddResultAndTransformAsync(await QueryPatron
                    .GetByPatronEmailAsync(db, userInfoAndStats.Email ?? "")))
                .OkOrDefault();
        }

        return Result<ModelResult<ViewProfileStats>, string>
            .Ok(ModelResult<ViewProfileStats>.Ok(new ViewProfileStats { 
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

    public static async Task<Result<ModelResult<ViewUserRankTimeLine>, string>> WebProfileStatsTimeLineAsync(
        ControllerExtensions controller, NpgsqlConnection db, long userId) {

        var result = await QueryGlobalRankingTimeline
            .BuildTimeLineAsync(db, userId, DateTime.UtcNow - TimeSpan.FromDays(90));
        
        if (result == EResult.Err)
            return result.ChangeOkType<ModelResult<ViewUserRankTimeLine>>();
        
        var rankingTimeline = result
                              .Ok()
                              .Select(x => new ViewUserRankTimeLine.RankTimeLineValue {
                                     Date = x.Date,
                                     Score = x.Score,
                                     Rank = x.GlobalRanking
                                 }).ToList();

        return Result<ModelResult<ViewUserRankTimeLine>, string>
            .Ok(ModelResult<ViewUserRankTimeLine>.Ok(new ViewUserRankTimeLine {
                UserId = userId,
                List = rankingTimeline
            }));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> UpdateEmailAsync(
        ControllerExtensions controller, NpgsqlConnection db, LamLog log, UpdateEmailDto updateEmail, UserIdAndToken cookieToken) {
        
        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);
        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();

        if (userInfoResult.Ok().IsNotSet())
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = false }));

        var userInfo = userInfoResult.Ok().Unwrap();

        var result = await OsuDroidLib.Manager.UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateEmail.Passwd, userInfo.Password ?? "");
        
        if (result == EResult.Err)
            return Result<ModelResult<ApiTypes.ViewWork>, string>.Err(result.Err());

        if (updateEmail.OldEmail != userInfo.Email)
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = false }));
        
        switch (result.Ok()) {
            case { PasswordIsValid: false }:
                return Result<ModelResult<ApiTypes.ViewWork>, string>
                    .Ok(ModelResult<ApiTypes.ViewWork>
                        .Ok(new ApiTypes.ViewWork() { HasWork = false }));
            default:
                if (result.Ok().RehashPassword)
                    await log.AddLogDebugAsync($"RehashPassword For UserId: {userInfo.UserId}");
                if (result.Ok().ChangeToBCrypt)
                    await log.AddLogDebugAsync($"ChangeToBCrypt For UserId: {userInfo.UserId}");
                break;
        }
        
        return Result<ModelResult<ApiTypes.ViewWork>, string>
            .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = true }));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> UpdatePasswdAsync(
        ControllerExtensions controller, NpgsqlConnection db, UpdatePasswdDto updatePasswd, UserIdAndToken cookieToken) {
        
        var resultUserInfo = await UserInfoManager.GetByUserIdAsync(db, cookieToken.UserId);
        if (resultUserInfo == EResult.Err)
            return resultUserInfo.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();
        
        
        var userInfoOption = resultUserInfo.Ok();
        if (userInfoOption.IsNotSet())
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.InternalServerError());

        
        
        var userInfo = userInfoOption.Unwrap();
        var rightPassword = OsuDroidLib.Lib.PasswordHash
                                       .IsRightPassword(updatePasswd.OldPasswd, userInfo.Password??"");

        if (rightPassword == EResult.Err)
            return rightPassword.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();
        
        if (!rightPassword.Ok())
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = false }));

        var newPasswordResult = OsuDroidLib.Lib.PasswordHash.HashWithBCryptPassword(updatePasswd.NewPasswd);
        if (newPasswordResult == EResult.Err) 
            return rightPassword.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();

        var result = await UserInfoManager.UpdatePasswordAsync(db, userInfo.UserId, newPasswordResult.Ok());
        if (result == EResult.Err)
            return result.ConvertTo<ModelResult<ApiTypes.ViewWork>>();
        return Result<ModelResult<ApiTypes.ViewWork>, string>
            .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = true }));
    }


    public static async Task<Result<ModelResult<ViewUpdateUsernameRes>, string>> UpdateUsernameAsync(
        ControllerExtensions controller, NpgsqlConnection db, UpdateUsernameDto updateUsername, UserIdAndToken cookieToken) {
        
        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ViewUpdateUsernameRes>>();

        if (userInfoResult.Ok().IsNotSet()) {
            return Result<ModelResult<ViewUpdateUsernameRes>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {cookieToken.UserId}"));
        }
            

        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateUsername.Passwd, userInfo.Password!);
        
        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<ModelResult<ViewUpdateUsernameRes>>();
        
        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<ModelResult<ViewUpdateUsernameRes>, string>.Ok(ModelResult<ViewUpdateUsernameRes>
                .Ok(new ViewUpdateUsernameRes() { HasWork = false }));
        
        var checkUsername = await UserInfoManager.UsernameIsInUse(db, updateUsername.NewUsername);

        if (checkUsername == EResult.Err)
            return checkUsername.ChangeOkType<ModelResult<ViewUpdateUsernameRes>>();
            
        if (checkUsername.Ok())
            return Result<ModelResult<ViewUpdateUsernameRes>, string>.Ok(ModelResult<ViewUpdateUsernameRes>
                .Ok(new ViewUpdateUsernameRes() { HasWork = false }));
            
        var result = await UserInfoManager.UpdateUsernameAsync(db, userInfo.UserId, updateUsername.NewUsername);

        if (result == EResult.Err)
            return result.ConvertTo<ModelResult<ViewUpdateUsernameRes>>();
            
        return Result<ModelResult<ViewUpdateUsernameRes>, string>.Ok(ModelResult<ViewUpdateUsernameRes>
            .Ok(new ViewUpdateUsernameRes() { HasWork = true }));
    }

    public static async Task<Result<ModelResult<ViewUpdateAvatar>, string>> UpdateAvatarAsync(
        ControllerExtensions controller, NpgsqlConnection db, UpdateAvatarDto updateAvatar, UserIdAndToken cookieToken) {
        
        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ViewUpdateAvatar>>();

        if (userInfoResult.Ok().IsNotSet()) {
            return Result<ModelResult<ViewUpdateAvatar>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {cookieToken.UserId}"));
        }
        
        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateAvatar.Passwd, userInfo.Password!);
        
        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<ModelResult<ViewUpdateAvatar>>();
        
        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<ModelResult<ViewUpdateAvatar>, string>.Ok(ModelResult<ViewUpdateAvatar>
                .Ok(new () { PasswdFalse = true, ImageToBig = false, IsNotAImage = false }));
        
            
        byte[] imageBytes = Array.Empty<byte>();
        try {
            var charArr = updateAvatar.ImageBase64.AsSpan(updateAvatar.ImageBase64!.IndexOf(',') + 1).ToArray();
            // TODO Write one Convert.FromBase64String With Span
            imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
        }
        catch (Exception e) {
            return Result<ModelResult<ViewUpdateAvatar>, string>.Err(e.ToString());
        }

        var result = await OsuDroidLib.Lib.UserAvatarHandler.UpdateImageForUserAsync(db, userInfo.UserId, imageBytes);

        if (result == EResult.Err)
            return result.ConvertTo<ModelResult<ViewUpdateAvatar>>();
            
        return Result<ModelResult<ViewUpdateAvatar>, string>
            .Ok(ModelResult<ViewUpdateAvatar>.Ok(new ViewUpdateAvatar {
                PasswdFalse = false,
                ImageToBig = false,
                IsNotAImage = false
            }));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> UpdatePatreonEmailAsync(ControllerExtensions controller, 
        NpgsqlConnection db, UpdatePatreonEmailDto updatePatreonEmail, UserIdAndToken cookieToken) {
        
        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();

        if (userInfoResult.Ok().IsNotSet()) {
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {cookieToken.UserId}"));
        }
        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updatePatreonEmail.Passwd, userInfo.Password!);
        
        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<ModelResult<ApiTypes.ViewWork>>();
        
        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<ModelResult<ApiTypes.ViewWork>, string>.Ok(ModelResult<ApiTypes.ViewWork>
                .Ok(new () { HasWork = false }));

        var token = Guid.NewGuid();

        _patreoneMailToken.Add(token, (cookieToken.UserId, updatePatreonEmail.Email!));

        SendEmail.MainSendPatreonVerifyLinkToken(userInfo.Username!, userInfo.Email!, token);

        return Result<ModelResult<ApiTypes.ViewWork>, string>
            .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new () { HasWork = true }));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> AcceptPatreonEmailAsync(
        ControllerExtensions controller, NpgsqlConnection db, Guid token) {
     
        var response = _patreoneMailToken.Pop(token);

        if (response.IsNotSet()) {
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.Ok(ApiTypes.ViewWork.False));
        }

        var userIdAndEmail = response.Unwrap();

        var resultErr = await QueryUserInfo.SetPatreonEmailAsync(db, userIdAndEmail.UserId, userIdAndEmail.Email);
        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<ModelResult<ApiTypes.ViewWork>>();

        var resultSetAccept = await QueryUserInfo.SetAcceptPatreonEmailAsync(db, userIdAndEmail.UserId);
        if (resultSetAccept == EResult.Err)
            return resultSetAccept.ConvertTo<ModelResult<ApiTypes.ViewWork>>();

        return Result<ModelResult<ApiTypes.ViewWork>, string>
            .Ok(ModelResult<ApiTypes.ViewWork>.Ok(ApiTypes.ViewWork.True));
    }

    public static async Task<Result<ModelResult<ViewCreateDropAccountTokenRes>, string>> CreateDropAccountTokenAsync(
        ControllerExtensions controller, NpgsqlConnection db, 
        CreateDropAccountTokenDto createDropAccountToken, UserIdAndToken cookieToken) {
        
        var optionBblUserResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);

        if (optionBblUserResult == EResult.Err)
            return optionBblUserResult.ChangeOkType<ModelResult<ViewCreateDropAccountTokenRes>>();
        
        if (optionBblUserResult.Ok().IsNotSet())
            return Result<ModelResult<ViewCreateDropAccountTokenRes>, string>
                .Ok(ModelResult<ViewCreateDropAccountTokenRes>.Ok(ViewCreateDropAccountTokenRes.HasElseError()));

        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieToken.UserId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ViewCreateDropAccountTokenRes>>();

        if (userInfoResult.Ok().IsNotSet()) {
            return Result<ModelResult<ViewCreateDropAccountTokenRes>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {cookieToken.UserId}"));
        }
        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, createDropAccountToken.Password, userInfo.Password!);
        
        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<ModelResult<ViewCreateDropAccountTokenRes>>();
        
        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<ModelResult<ViewCreateDropAccountTokenRes>, string>.Ok(ModelResult<ViewCreateDropAccountTokenRes>
                .Ok(ViewCreateDropAccountTokenRes.PasswordIsFalse()));


        var deleteAccToken = Guid.NewGuid();
        _deleteAccMailToken.Add(deleteAccToken, (userInfo.UserId, userInfo.Email??""));
        Utils.SendEmail.MainSendDropAccountVerifyLinkToken(userInfo.Username??"", userInfo.Email??"", deleteAccToken);

        return Result<ModelResult<ViewCreateDropAccountTokenRes>, string>
            .Ok(ModelResult<ViewCreateDropAccountTokenRes>.Ok(ViewCreateDropAccountTokenRes.NoError()));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> DropAccountWithTokenAsync(
        ControllerExtensions controller, NpgsqlConnection db, Guid token, UserIdAndToken cookieToken) {
      
        _deleteAccMailToken.CleanDeadTokens();

        
        var deleteAccTokenResponse = _deleteAccMailToken.Pop(token: token);
        if (deleteAccTokenResponse.IsNotSet())
            return Result<ModelResult<ApiTypes.ViewWork>, string>.Ok(ModelResult<ApiTypes.ViewWork>.BadRequest());

        var userId = deleteAccTokenResponse.Unwrap().UserId;
        if (userId != cookieToken.UserId)
            return Result<ModelResult<ApiTypes.ViewWork>, string>.Ok(ModelResult<ApiTypes.ViewWork>.BadRequest());
        
        var deleteAccountResponse = await QueryUserInfo.DeleteAsync(db, userId);
        if (deleteAccountResponse == EResult.Err)
            return deleteAccountResponse.ConvertTo<ModelResult<ApiTypes.ViewWork>>();
            
        return Result<ModelResult<ApiTypes.ViewWork>, string>.Ok(ModelResult<ApiTypes.ViewWork>.Ok(ApiTypes.ViewWork.True));
    }

    public static async Task<Result<ModelResult<ViewPlaysMarksLength>, string>> WebProfileTopPlaysByMarksLengthAsync(
        ControllerExtensions controller, NpgsqlConnection db, long userId) {
        
        var result = await QueryPlayScore.CountMarkPlaysByUserIdAsync(db, userId);
        if (result == EResult.Err)
            return result.ChangeOkType<ModelResult<ViewPlaysMarksLength>>();
        
        Dictionary<PlayScoreDto.EPlayScoreMark, long> dic = result.Ok();
            
        return Result<ModelResult<ViewPlaysMarksLength>, string>
            .Ok(ModelResult<ViewPlaysMarksLength>.Ok(ViewPlaysMarksLength.Factory(dic)));
    }
}












































