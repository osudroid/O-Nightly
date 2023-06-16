using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Lib.Validate;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api;

#nullable enable

public sealed partial class Login : ControllerExtensions {
    private static readonly ConcurrentDictionary<IPAddress, (DateTime LastCall, int Calls)> CallsForResetPasswd = new();

    /// <summary> Key Token Value CreateTime </summary>
    private static readonly ConcurrentDictionary<string, (DateTime, long UserId)> ResetPasswdTime = new();

    private static readonly Random Random = new();
    private static readonly ConcurrentDictionary<Guid, (ViewWebLoginToken, DateTime)> TokenDic = new();

    [HttpPost("/api/weblogin")]
    [PrivilegeRoute(route: "/api/weblogin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebLogin([FromBody] WebLoginProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var now = DateTime.UtcNow;
            {
                foreach (var token in TokenDic.Keys)
                    if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                        TokenDic.TryRemove(token, out var _);
            }


            if (TokenDic.Remove(prop.Token, out var tokenAndTime) == false)
                return BadRequest();

            var tokenValue = tokenAndTime.Item1;
            if (prop.Math != tokenValue.MathValue1 + tokenValue.MathValue2)
                return Ok(new ViewWebLogin { Work = false });

            var fetchResult = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetLoginInfoByEmailAndPasswordByEmailAndPasswordAsync(
                db,
                (prop.Email??"").ToLower(),
                ToPasswdHash(prop.Passwd??"")
            ));

            if (fetchResult == EResult.Err)
                return GetInternalServerError();
            
            
            if (fetchResult.Ok().IsNotSet())
                return Ok(new ViewWebLogin { Work = false });

            var userInfo = fetchResult.Ok().Unwrap();

            var tokenResult  = await log.AddResultAndTransformAsync(
                await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(db, userInfo.UserId));
            
            if (tokenResult == EResult.Err)
                return GetInternalServerError();
            
            
            AppendCookie(ECookie.LoginCookie, tokenResult.Ok().ToString());

            
            await log.AddResultAndTransformAsync(await QueryUserInfo.UpdateLastLoginTimeAsync(db, userInfo.UserId));
            var ipOption = (await log.AddResultAndTransformAsync(GetIpAddress())).OkOrDefault();
            if (ipOption.IsSet())
                await UserInfoHandler.UpdateIpAndRegionByIpAsync(db, userInfo, ipOption.Unwrap());
            return Ok(new ViewWebLogin { Work = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("/api/webloginwithusername")]
    [PrivilegeRoute(route: "/api/webloginwithusername")]
    public async Task<IActionResult> WebLoginWithUsername([FromBody] WebLoginWithUsernameProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            prop.Username = this.FixUsername(prop.Username ?? string.Empty);

            var now = DateTime.UtcNow;
            {
                foreach (var token in TokenDic.Keys)
                    if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                        TokenDic.TryRemove(token, out var _);
            }


            if (TokenDic.Remove(prop.Token, out var tokenAndTime) == false)
                return BadRequest();

            var tokenValue = tokenAndTime.Item1;
            if (prop.Math != tokenValue.MathValue1 + tokenValue.MathValue2)
                return Ok(new ViewWebLogin { Work = false });


            var passwdHash = this.ToPasswdHash(prop.Passwd ?? string.Empty);
            
            var fetchResult = await log.AddResultAndTransformAsync(await QueryUserInfo
                .GetLoginInfoByEmailAndPasswordByUsernameAndPasswordAsync(db, prop.Username.ToLower(), passwdHash));

            if (fetchResult == EResult.Err)
                return GetInternalServerError();
            
            if (fetchResult.Ok().IsNotSet())
                return Ok(new ViewWebLogin { Work = false });

            var loginInfo = fetchResult.Ok().Unwrap();
            var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
            var resultGuid = await log.AddResultAndTransformAsync(
                await tokenHandler.InsertAsync(db, loginInfo.UserId));
            if (resultGuid == EResult.Err)
                return GetInternalServerError();
            AppendCookie(ECookie.LoginCookie, resultGuid.Ok().ToString());

            await log.AddResultAndTransformAsync(await QueryUserInfo.UpdateLastLoginTimeAsync(db, loginInfo.UserId));
            await log.AddResultAndTransformAsync(await QueryUserInfo.UpdateLastLoginTimeAsync(db, loginInfo.UserId));
            var ipOption = (await log.AddResultAndTransformAsync(GetIpAddress())).OkOrDefault();
            if (ipOption.IsSet())
                await UserInfoHandler.UpdateIpAndRegionByIpAsync(db, fetchResult.Ok().Unwrap(), ipOption.Unwrap());

            return Ok(new ViewWebLogin
                { Work = true, EmailExist = true, UsernameExist = true, UserOrPasswdOrMathIsFalse = false });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/webregister")]
    [PrivilegeRoute(route: "/api/webregister")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebLogin))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> WebRegister([FromBody] WebRegisterProp value) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (value.AnyValidate() == EResult.Err) {
                return Ok(new ViewWebLogin { UserOrPasswdOrMathIsFalse = true });
            }

            value.Username = this.FixUsername(value.Username ?? string.Empty);

            var now = DateTime.UtcNow;
            {
                foreach (var token in TokenDic.Keys)
                    if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                        TokenDic.TryRemove(token, out var _);
            }

            (ViewWebLoginToken?, DateTime) tokenValue = (default, default);

            if (TokenDic.TryRemove(value.MathToken, out tokenValue!) == false
                || tokenValue.Item1!.MathValue1 + tokenValue.Item1.MathValue2 != value.MathRes
               ) 
                return Ok(new ViewWebLogin { UserOrPasswdOrMathIsFalse = true });

            var findResult = await log.AddResultAndTransformAsync(
                await QueryUserInfo.GetEmailAndUsernameByEmailAndUsername(db, value.Email??"", value.Username??""));

            if (findResult == EResult.Err)
                return GetInternalServerError();

            var find = Option<UserInfo>.NullSplit(findResult.Ok().FirstOrDefault());
            
            if (find.IsSet()) {
                if (find.Unwrap().Username == value.Username)
                    return Ok(new ViewWebLogin { UsernameExist = true });
                if (find.Unwrap().Email == value.Email)
                    return Ok(new ViewWebLogin { EmailExist = true });
            }

            var optionIp = (await log.AddResultAndTransformAsync(GetIpAddress())).OkOr(Option<IPAddress>.Empty);
            if (optionIp.IsSet() == false) {
                await log.AddLogErrorAsync("ip not found", Option<string>.With(TraceMsg.WithMessage("ip not found")));
                throw new Exception("ip not found");
            }
            
            var ip = optionIp.Unwrap();


            var optionCountry = CountryInfo.FindByName((IpInfo.Country(ip)?.Country.Name) ?? "");
            var newUser = new Entities.UserInfo {
                Active = true,
                Banned = false,
                DeviceId = "",
                Email = (value.Email??"").ToLower(),
                Password = this.ToPasswdHash(value.Passwd ?? string.Empty),
                Username = value.Username,
                Region = optionCountry.IsSet() ? optionCountry.Unwrap().NameShort: "",
                LatestIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                RegisterTime = DateTime.UtcNow,
                RestrictMode = false,
                LastLoginTime = DateTime.UtcNow,
                UsernameLastChange = DateTime.UtcNow
            };
            await QueryUserInfo.InsertAsync(db, newUser);
            var userIdOpt = (await log.AddResultAndTransformAsync(await QueryUserInfo.GetUserIdByUsernameAsync(db, newUser.Username!)))
                .Map(x => x.IsNotSet() ? Option<long>.Empty : Option<long>.With(x.Unwrap().UserId)).OkOr(Option<long>.Empty);

            if (userIdOpt.IsNotSet())
                return GetInternalServerError();


            if (await log.AddResultAndTransformAsync<string>(
                    await QueryUserStats.InsertAsync(db, new() { UserId = userIdOpt.Unwrap() })) == EResult.Err) {
                return GetInternalServerError();
            }
            
            return Ok(new ViewWebLogin { Work = true });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/weblogintoken")]
    [PrivilegeRoute(route: "/api/weblogintoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebLoginToken))]
    public async Task<IActionResult> WebLoginToken() {
         await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var res = new ViewWebLoginToken {
                Token = Guid.NewGuid(),
                MathValue1 = Random.Next(1, 50),
                MathValue2 = Random.Next(1, 50)
            };
            TokenDic[res.Token] = (res, DateTime.UtcNow + TimeSpan.FromMinutes(5));
            return Ok(res);
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/webupdateCookie")]
    [PrivilegeRoute(route: "/api/webupdateCookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<ViewUpdateCookieInfo>))]
    public async Task<IActionResult> WebUpdateCookie() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var cookieInfo = this.LoginTokenInfo(db).Ok().Unwrap();
            this.AppendCookie(ECookie.LoginCookie, cookieInfo.Token.ToString());


            var userInfoOption = (await log.AddResultAndTransformAsync(await QueryUserInfo
                .GetByUserIdAsync(db, cookieInfo.UserId)))
                .OkOrDefault();
            if (userInfoOption.IsNotSet())
                return GetInternalServerError();
            
            var userInfo = userInfoOption.Unwrap();
            
            return Ok(new ApiTypes.ExistOrFoundInfo<ViewUpdateCookieInfo> {
                ExistOrFound = true, Value = new () {
                    Email = userInfo.Email!,
                    Username = userInfo.Username!
                }
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
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

    [HttpPost("/api/webresetpasswdandsendemail")]
    [PrivilegeRoute(route: "/api/webresetpasswdandsendemail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewResetPasswdAndSendEmail))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewResetPasswdAndSendEmail))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswdAndSendEmail([FromBody] ResetPasswdAndSendEmailProp prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.AnyValidate() == EResult.Err) {
                return Ok(new ViewResetPasswdAndSendEmail { Work = false, TimeOut = false });
            }

            Option<IPAddress> optionIpAddress = Option<IPAddress>.Trim(await log.AddResultAndTransformAsync(GetIpAddress()));
            if (optionIpAddress.IsNotSet()) {
                return BadRequest("Can Not Get IP IS NEEDED");
            }
                
            var ipAddress = optionIpAddress.Unwrap();

            FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

            if (CallsForResetPasswd.TryGetValue(ipAddress, out var lastCall)) {
                if (lastCall.Calls > 3)
                    return Ok(new ViewResetPasswdAndSendEmail { Work = false, TimeOut = true });
                CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
            }
            else {
                CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
            }

            if (Email.ValidateEmail(prop.Email!) == false)
                return Ok(new ViewResetPasswdAndSendEmail { Work = false, TimeOut = false });

            var userInfoResult = (await log.AddResultAndTransformAsync(string.IsNullOrEmpty(prop.Email) switch {
                true => await QueryUserInfo.GetByUsernameAsync(db, prop.Username ?? ""),
                _ => await QueryUserInfo.GetByEmailAsync(db, prop.Email ?? "")
            }));

            if (userInfoResult == EResult.Err)
                return GetInternalServerError();
            var userInfoOption = userInfoResult.Ok(); 
            
            if (userInfoOption.IsNotSet())
                return Ok(new ViewResetPasswdAndSendEmail { Work = false, TimeOut = false });
            var userInfo = userInfoOption.Unwrap();
            var token = RandomText.NextAZ09(12);
            
            ResetPasswdTime[token] = (DateTime.UtcNow, userInfo.UserId);
            SendEmail.MainSendResetEmail(userInfo.UserId, userInfo.Username!, userInfo.Email!, token);

            return Ok(new ViewResetPasswdAndSendEmail { Work = true, TimeOut = false });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/token/newpasswdwithtoken")]
    [PrivilegeRoute(route: "/api/token/newpasswdwithtoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewWebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ViewWebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetNewPasswd([FromBody] ApiTypes.Api2GroundNoHeader<SetNewPasswdProp> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false) {
                return Ok(new ViewWebReplacePasswordWithToken {
                    Work = false,
                    ErrorMsg = "Send Error"
                });
            }

            FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

            var body = prop.Body!;

            if (ResetPasswdTime.TryGetValue(body.Token!, out var tokenValue) == false
                || tokenValue.UserId != body.UserId)
                return Ok(new ViewWebReplacePasswordWithToken {
                    Work = false,
                    ErrorMsg = "Token To Old Or User Not Exist"
                });

            if (body.NewPasswd is null || body.NewPasswd.Length < 6)
                return Ok(new ViewWebReplacePasswordWithToken {
                    Work = false,
                    ErrorMsg = "Password To Short"
                });
            if (await log.AddResultAndTransformAsync<string>(
                    await QueryUserInfo.UpdatePasswordAsync(db, tokenValue.UserId, ToPasswdHash(body.NewPasswd))) ==
                EResult.Err) {

                return GetInternalServerError();
            }
            
            return Ok(new ViewWebReplacePasswordWithToken {
                Work = true,
                ErrorMsg = ""
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api/signin/patreon")]
    [PrivilegeRoute(route: "/api/signin/patreon")]
    public async Task<IActionResult> PatreonSignLogin([FromForm] string provider) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
                return BadRequest();

            if (await HttpContext.IsProviderSupportedAsync(provider))
                return BadRequest();

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api/signout/patreon")]
    [PrivilegeRoute(route: "/api/signout/patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatreonSignout() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            // TODO PatreonSignout


            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return Ok();
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }


    [HttpGet("/api/weblogout")]
    [PrivilegeRoute(route: "/api/weblogout")]
    public async Task<IActionResult> RemoveCookie() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            RemoveCookieByEName(ECookie.LoginCookie);
            return Ok();
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }


    public sealed class SetNewPasswdProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString {
        [FromBody] public string? NewPasswd { get; set; }
        [FromBody] public string? Token { get; set; }
        [FromBody] public long UserId { get; set; }

        public string ToSingleString() {
            return Merge.ObjectsToString(new[] {
                NewPasswd ?? "",
                Token ?? "",
                UserId.ToString()
            });
        }

        public bool ValuesAreGood() {
            return !(string.IsNullOrEmpty(NewPasswd)
                     || string.IsNullOrEmpty(Token)
                     || UserId < 0);
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class ResetPasswdAndSendEmailProp : ValidateAll, IValidateEmail, IValidateUsername {
        public string? Email { get; set; }
        public string? Username { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebLoginWithUsernameProp : ValidateAll, IValidatePasswd, IValidateUsername {
        public int Math { get; set; }
        public Guid Token { get; set; }
        public string? Passwd { get; set; }
        public string? Username { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebRegisterProp : ValidateAll, IValidatePasswd, IValidateUsername {
        public string? Email { get; set; }
        public int MathRes { get; set; }
        public Guid MathToken { get; set; }
        public string? Region { get; set; }
        public string? Passwd { get; set; }
        public string? Username { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebLoginProp : ValidateAll, IValidatePasswd, IValidateEmail {
        public int Math { get; set; }
        public Guid Token { get; set; }
        public string? Email { get; set; }
        public string? Passwd { get; set; }
    }
}