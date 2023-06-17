using System.Collections.Concurrent;
using Npgsql;
using OsuDroid.Extensions;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Post;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Lib.Validate;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Model; 

public static class ModelApiLogin {
    private static readonly ConcurrentDictionary<IPAddress, (DateTime LastCall, int Calls)> CallsForResetPasswd = new();

    /// <summary> Key Token Value CreateTime </summary>
    private static readonly ConcurrentDictionary<string, (DateTime, long UserId)> ResetPasswdTime = new();

    private static readonly Random Random = new();
    private static readonly ConcurrentDictionary<Guid, (ViewWebLoginToken, DateTime)> TokenDic = new();
    
    public static async Task<Result<ModelResult<ViewWebLogin>, string>> WebLogin(
        ControllerExtensions controller, NpgsqlConnection db, WebLoginDto webLogin) {
        
        var now = DateTime.UtcNow;
        {
            foreach (var token in TokenDic.Keys)
                if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                    TokenDic.TryRemove(token, out var _);
        }


        if (TokenDic.Remove(webLogin.Token, out var tokenAndTime) == false)
            return Result<ModelResult<ViewWebLogin>, string>.Ok(ModelResult<ViewWebLogin>.BadRequest());

        var tokenValue = tokenAndTime.Item1;
        if (webLogin.Math != tokenValue.MathValue1 + tokenValue.MathValue2)
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .Ok(new ViewWebLogin { Work = false }));
        

        var fetchResult = await QueryUserInfo.GetLoginInfoByEmailAsync(
            db, 
            webLogin.Email.ToLower()
        );

        if (fetchResult == EResult.Err)
            return fetchResult.ChangeOkType<ModelResult<ViewWebLogin>>();
            
        
        if (fetchResult.Ok().IsNotSet())
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .Ok(new ViewWebLogin { Work = false }));

        var userInfo = fetchResult.Ok().Unwrap();

        if (PasswordHash.IsBCryptHash(userInfo.Password!)) {
            var rightPasswordResult = PasswordHash.IsRightPassword(webLogin.Passwd!, userInfo.Password!);
            if (rightPasswordResult == EResult.Err) 
                return rightPasswordResult.ChangeOkType<ModelResult<ViewWebLogin>>();
            
            if (!rightPasswordResult.Ok())
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { Work = false }));
        }
        else {
            var passwdHash = PasswordHash.HashWithOldPassword(webLogin.Passwd!);
            if (passwdHash != userInfo.Password)   
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { Work = false }));

            var updatePassword = await UserInfoManager.UpdatePasswordAsync(db, userInfo.UserId, webLogin.Passwd);
            if (updatePassword == EResult.Err)
                return updatePassword.ConvertTo<ModelResult<ViewWebLogin>>();
        }
        
        var tokenResult  = await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(db, userInfo.UserId);
            
        if (tokenResult == EResult.Err)
            return tokenResult.ChangeOkType<ModelResult<ViewWebLogin>>();
            
            
        controller.AppendCookie(ControllerExtensions.ECookie.LoginCookie, tokenResult.Ok().ToString());
        return Result<ModelResult<ViewWebLogin>, string>
            .Ok(ModelResult<ViewWebLogin>
                .Ok(new ViewWebLogin { Work = true }));
    }

    public static async Task<Result<ModelResult<ViewWebLogin>, string>> WebLoginWithUsername(
        ControllerExtensions controller, NpgsqlConnection db, WebLoginWithUsernameDto webLogin) { 
        
        var now = DateTime.UtcNow; 
        {
            foreach (var token in TokenDic.Keys)
                if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                    TokenDic.TryRemove(token, out var _);
        }


        if (TokenDic.Remove(webLogin.Token, out var tokenAndTime) == false) {
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>.BadRequest());
        }
            

        var tokenValue = tokenAndTime.Item1;
        if (webLogin.Math != tokenValue.MathValue1 + tokenValue.MathValue2)
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .Ok(new ViewWebLogin { Work = false }));

            
        var fetchResult = await QueryUserInfo.GetLoginInfoByByUsernameAsync(db, webLogin.Username.ToLower());

        if (fetchResult == EResult.Err)
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .InternalServerError());
            
        if (fetchResult.Ok().IsNotSet())
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .Ok(new ViewWebLogin { Work = false }));

        var userInfo = fetchResult.Ok().Unwrap();
        
        
        if (PasswordHash.IsBCryptHash(userInfo.Password!)) {
            var rightPasswordResult = PasswordHash.IsRightPassword(webLogin.Passwd!, userInfo.Password!);
            if (rightPasswordResult == EResult.Err) 
                return rightPasswordResult.ChangeOkType<ModelResult<ViewWebLogin>>();
            
            if (!rightPasswordResult.Ok())
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { Work = false }));
        }
        else {
            var passwdHash = PasswordHash.HashWithOldPassword(webLogin.Passwd!);
            if (passwdHash != userInfo.Password)   
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { Work = false }));

            var updatePassword = await UserInfoManager.UpdatePasswordAsync(db, userInfo.UserId, webLogin.Passwd);
            if (updatePassword == EResult.Err)
                return updatePassword.ConvertTo<ModelResult<ViewWebLogin>>();
        }
        
        
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var resultGuid = await tokenHandler.InsertAsync(db, userInfo.UserId);
        if (resultGuid == EResult.Err)
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .InternalServerError());
        
        controller.AppendCookie(ControllerExtensions.ECookie.LoginCookie, resultGuid.Ok().ToString());

        return Result<ModelResult<ViewWebLogin>, string>
            .Ok(ModelResult<ViewWebLogin>
                .Ok(new ViewWebLogin {
                    Work = true, 
                    EmailExist = true, 
                    UsernameExist = true, 
                    UserOrPasswdOrMathIsFalse = false
                }));
    }

    public static async Task<Result<ModelResult<ViewWebLogin>, string>> WebRegisterAsync(
        ControllerExtensions controller, NpgsqlConnection db, WebRegisterDto webRegister) {
        
        var now = DateTime.UtcNow;
        {
            foreach (var token in TokenDic.Keys)
                if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                    TokenDic.TryRemove(token, out var _);
        }

        (ViewWebLoginToken?, DateTime) tokenValue = default;

        if (TokenDic.TryRemove(webRegister.MathToken, out tokenValue!) == false
            || tokenValue.Item1!.MathValue1 + tokenValue.Item1.MathValue2 != webRegister.MathRes
           ) 
            return Result<ModelResult<ViewWebLogin>, string>
                .Ok(ModelResult<ViewWebLogin>
                    .Ok(new () { UserOrPasswdOrMathIsFalse = true }));

        var findResult = await QueryUserInfo.GetEmailAndUsernameByEmailAndUsername(db, webRegister.Email, webRegister.Username);

        if (findResult == EResult.Err)
            return findResult.ChangeOkType<ModelResult<ViewWebLogin>>();

        var find = Option<UserInfo>.NullSplit(findResult.Ok().FirstOrDefault());
            
        if (find.IsSet()) {
            if (find.Unwrap().Username == webRegister.Username)
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { UsernameExist = true }));
            if (find.Unwrap().Email == webRegister.Email)
                return Result<ModelResult<ViewWebLogin>, string>
                    .Ok(ModelResult<ViewWebLogin>
                        .Ok(new ViewWebLogin { EmailExist = true }));
        }

        var optionIp = controller.GetIpAddress().OkOr(Option<IPAddress>.Empty);
        if (optionIp.IsSet() == false) {
            return Result<ModelResult<ViewWebLogin>, string>.Err(TraceMsg.WithMessage("ip not found"));
        }
            
        var ip = optionIp.Unwrap();

        
        var optionCountry = CountryInfo.FindByName((IpInfo.Country(ip)?.Country.Name) ?? "");
        var newUser = new Entities.UserInfo {
            Active = true,
            Banned = false,
            DeviceId = "",
            Email = (webRegister.Email??"").ToLower(),
            Password = PasswordHash.HashWithBCryptPassword(webRegister.Passwd).Ok(),
            Username = webRegister.Username,
            Region = optionCountry.IsSet() ? optionCountry.Unwrap().NameShort: "",
            LatestIp = controller.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            RegisterTime = DateTime.UtcNow,
            RestrictMode = false,
            LastLoginTime = DateTime.UtcNow,
            UsernameLastChange = DateTime.UtcNow
        };
        var resultErr = await QueryUserInfo.InsertAsync(db, newUser);
        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<ModelResult<ViewWebLogin>>();
        
        var userIdOpt = (await QueryUserInfo.GetUserIdByUsernameAsync(db, newUser.Username))
                        .Map(x => x.IsNotSet() ? Option<long>.Empty : Option<long>.With(x.Unwrap().UserId)).OkOr(Option<long>.Empty);

        if (userIdOpt.IsNotSet())
            return Result<ModelResult<ViewWebLogin>, string>.Err(TraceMsg.WithMessage("userIdOpt Is Not Set"));

        resultErr = await QueryUserStats.InsertAsync(db, new() { UserId = userIdOpt.Unwrap() });

        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<ModelResult<ViewWebLogin>>();
            
        return Result<ModelResult<ViewWebLogin>, string>
            .Ok(ModelResult<ViewWebLogin>
                .Ok(new ViewWebLogin { Work = true }));
    }

    public static async Task<Result<ModelResult<ViewWebLoginToken>, string>> WebLoginTokenAsync(
        ControllerExtensions controller, NpgsqlConnection db) {
        
        try
        {
            var res = new ViewWebLoginToken {
                Token = Guid.NewGuid(),
                MathValue1 = Random.Next(1, 50),
                MathValue2 = Random.Next(1, 50)
            };
            TokenDic[res.Token] = (res, DateTime.UtcNow + TimeSpan.FromMinutes(5));
            return Result<ModelResult<ViewWebLoginToken>, string>
                .Ok(ModelResult<ViewWebLoginToken>.Ok(res));
        }
        catch (Exception e)
        {
            return Result<ModelResult<ViewWebLoginToken>, string>
                .Err(e.ToString());
        }
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>, string>> WebUpdateCookieAsync(
        ControllerExtensions controller, NpgsqlConnection db, UserIdAndToken cookieInfo) {

        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieInfo.UserId);
        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>>();
        
        
        
        var userInfoOption = userInfoResult.Ok();
        if (userInfoOption.IsNotSet())
            return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>, string>.Ok(
                ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>.InternalServerError());
            
        var userInfo = userInfoOption.Unwrap();
        
        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>, string>.Ok(
            ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo>>.Ok(
                new ApiTypes.ViewExistOrFoundInfo<ViewUpdateCookieInfo> {
                ExistOrFound = true, Value = new () {
                    Email = userInfo.Email!,
                    Username = userInfo.Username!
                }
            }));
    }

    public static async Task<Result<ModelResult<ViewResetPasswdAndSendEmail>, string>> ResetPasswdAndSendEmailAsync(
        ControllerExtensions controller, NpgsqlConnection db, ResetPasswdAndSendEmailDto prop, IPAddress ipAddress) {
        
        FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

        if (CallsForResetPasswd.TryGetValue(ipAddress, out var lastCall)) {
            if (lastCall.Calls > 3)
                Result<ModelResult<ViewResetPasswdAndSendEmail>, string>
                    .Ok(ModelResult<ViewResetPasswdAndSendEmail>
                        .Ok(new ViewResetPasswdAndSendEmail { Work = false, TimeOut = true }));
                
            CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
        }
        else {
            CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
        }

        var userInfoResult = (prop.Email == "") switch {
            true => await QueryUserInfo.GetByUsernameAsync(db, prop.Username),
            _ => await QueryUserInfo.GetByEmailAsync(db, prop.Email)
        };

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<ModelResult<ViewResetPasswdAndSendEmail>>();
        
        var userInfoOption = userInfoResult.Ok(); 
            
        if (userInfoOption.IsNotSet())
            return Result<ModelResult<ViewResetPasswdAndSendEmail>, string>
                .Ok(ModelResult<ViewResetPasswdAndSendEmail>.Ok(
                    new ViewResetPasswdAndSendEmail { Work = false, TimeOut = false }));
        
        var userInfo = userInfoOption.Unwrap();
        var token = RandomText.NextAZ09(12);
            
        ResetPasswdTime[token] = (DateTime.UtcNow, userInfo.UserId);
        SendEmail.MainSendResetEmail(userInfo.UserId, userInfo.Username!, userInfo.Email!, token);

        return Result<ModelResult<ViewResetPasswdAndSendEmail>, string>
            .Ok(ModelResult<ViewResetPasswdAndSendEmail>.Ok(
                new ViewResetPasswdAndSendEmail { Work = true, TimeOut = false }));
    }

    public static async Task<Result<ModelResult<ViewWebReplacePasswordWithToken>, string>> SetNewPasswdAsync(
        ControllerExtensions controller, NpgsqlConnection db, SetNewPasswdDto setNewPasswd) {
        
        FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

        var body = setNewPasswd;

        if (ResetPasswdTime.TryGetValue(body.Token!, out var tokenValue) == false
            || tokenValue.UserId != body.UserId)
            return Result<ModelResult<ViewWebReplacePasswordWithToken>, string>.Ok(
                ModelResult<ViewWebReplacePasswordWithToken>.Ok(new ViewWebReplacePasswordWithToken {
                    Work = false,
                    ErrorMsg = "Token To Old Or User Not Exist"
                }));

        var resultErr = await UserInfoManager.UpdatePasswordAsync(db, tokenValue.UserId, body.NewPasswd);
        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<ModelResult<ViewWebReplacePasswordWithToken>>();
        
        return Result<ModelResult<ViewWebReplacePasswordWithToken>, string>
            .Ok(ModelResult<ViewWebReplacePasswordWithToken>
                .Ok(new ViewWebReplacePasswordWithToken {
                    Work = true,
                    ErrorMsg = ""
                }));
    }
    
    private static void FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime() {
        var timeNow = DateTime.UtcNow;
        foreach (var (key, value) in CallsForResetPasswd)
            if (value.LastCall + TimeSpan.FromHours(1) <= timeNow)
                CallsForResetPasswd.Remove(key, out var _);

        foreach (var (key, value) in ResetPasswdTime)
            if (value.Item1 + TimeSpan.FromHours(15) <= timeNow)
                ResetPasswdTime.Remove(key, out var _);
    }
}