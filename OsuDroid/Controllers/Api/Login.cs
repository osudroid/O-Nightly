using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using OsuDroid.Extensions;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Lib.Validate;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api;

#nullable enable

public sealed class Login : ControllerExtensions {
    private static readonly ConcurrentDictionary<IPAddress, (DateTime LastCall, int Calls)> CallsForResetPasswd = new();

    /// <summary> Key Token Value CreateTime </summary>
    private static readonly ConcurrentDictionary<string, (DateTime, long UserId)> ResetPasswdTime = new();

    private static readonly Random Random = new();
    private static readonly ConcurrentDictionary<Guid, (WebLoginTokenRes, DateTime)> TokenDic = new();

    [HttpPost("/api/weblogin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebLogin([FromBody] WebLoginProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
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
            return Ok(new WebLoginRes { Work = false });
        
        BblUser? fetchResult = log.AddResultAndTransform(db.SingleOrDefault<Entities.BblUser>(
            "SELECT id, email, password FROM bbl_user WHERE email = @0 AND banned = false AND password = @1 LIMIT 1",
            prop.Email ?? "", this.ToPasswdHash(prop.Passwd ?? string.Empty))).OkOrDefault();
        
        if (fetchResult is null)
            return Ok(new WebLoginRes { Work = false });
        
        var optionGuid = Option<Guid>.Trim(log.AddResultAndTransform(TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User).Insert(db, fetchResult.Id)));
        if (optionGuid.IsSet() == false)
            return GetInternalServerError();
        AppendCookie(ECookie.LoginCookie, optionGuid.Unwrap().ToString());

        Database.TableFn.BblUser.UpdateLastLoginTime(fetchResult, db);
        var ip = log.AddResultAndTransform(GetIpAddress()).OkOr(Option<IPAddress>.Empty);
        if (ip.IsSet())
            Database.TableFn.BblUser.UpdateIpAndRegionByIp(fetchResult, db, ip.Unwrap());
        return Ok(new WebLoginRes { Work = true });
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("/api/webloginwithusername")]
    public IActionResult WebLoginWithUsername([FromBody] WebLoginWithUsernameProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
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
            return Ok(new WebLoginRes { Work = false });

        
        var passwdHash = this.ToPasswdHash(prop.Passwd ?? string.Empty);
        var fetchResult = log.AddResultAndTransform(db.SingleOrDefault<Entities.BblUser>(
            "SELECT id, email, password FROM bbl_user WHERE lower(username) = @0 AND banned = false AND password = @1 LIMIT 1",
            (prop.Username ?? "").ToLower(), passwdHash)).OkOrDefault();

        if (fetchResult is null)
            return Ok(new WebLoginRes { Work = false });

        var resultGuid = log.AddResultAndTransform(TokenHandlerManger.GetOrCreateCacheDatabase(ETokenHander.User).Insert(db, fetchResult.Id));
        if (resultGuid == EResult.Err)
            return GetInternalServerError();
        AppendCookie(ECookie.LoginCookie, resultGuid.Ok().ToString());

        Database.TableFn.BblUser.UpdateLastLoginTime(fetchResult, db);
        var optionIp = log.AddResultAndTransform(GetIpAddress()).OkOr(Option<IPAddress>.Empty);
        if (optionIp.IsSet())
            Database.TableFn.BblUser.UpdateIpAndRegionByIp(fetchResult, db, optionIp.Unwrap());

        return Ok(new WebLoginRes
            { Work = true, EmailExist = true, UsernameExist = true, UserOrPasswdOrMathIsFalse = false });
    }

    [HttpPost("/api/webregister")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(WebLoginRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult WebRegister([FromBody] WebRegisterProp value) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (value.AnyValidate() == EResult.Err) {
            return Ok(new WebLoginRes { UserOrPasswdOrMathIsFalse = true });
        }

        value.Username = this.FixUsername(value.Username ?? string.Empty);

        var now = DateTime.UtcNow;
        {
            foreach (var token in TokenDic.Keys)
                if (TokenDic.TryGetValue(token, out var valueTuple) && valueTuple.Item2 < now)
                    TokenDic.TryRemove(token, out var _);
        }

        (WebLoginTokenRes?, DateTime) tokenValue = (default, default);

        if (TokenDic.TryRemove(value.MathToken, out tokenValue!) == false
            || tokenValue.Item1!.MathValue1 + tokenValue.Item1.MathValue2 != value.MathRes
           )
            return Ok(new WebLoginRes { UserOrPasswdOrMathIsFalse = true });

        var sql = new Sql(@"
SELECT username, email FROM public.bbl_user
WHERE email = @0 
or lower(username) = @1
", value.Username.ToLower(), value.Email);
        
        var userExist = log.AddResultAndTransform(db.SingleOrDefault<Entities.BblUser>(sql)).OkOrDefault();
        if (userExist is not null) {
            if (userExist.Username == value.Username)
                return Ok(new WebLoginRes { UsernameExist = true });
            if (userExist.Email == value.Email)
                return Ok(new WebLoginRes { EmailExist = true });
        }

        var optionIp = log.AddResultAndTransform(GetIpAddress()).OkOr(Option<IPAddress>.Empty);
        if (optionIp.IsSet() == false)
            throw new Exception("ip not found");
        var ip = optionIp.Unwrap();
        
        
        var optionCountry = CountryInfo.FindByName((IpInfo.Country(ip)?.Country.Name) ?? "");
        var newUser = new Entities.BblUser {
            Active = true,
            Banned = false,
            Deviceid = "",
            Email = value.Email,
            Password = this.ToPasswdHash(value.Passwd ?? string.Empty),
            Username = value.Username,
            Region = optionCountry.IsSet() ? optionCountry.Unwrap().NameShort: "",
            LatestIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            RegistTime = DateTime.UtcNow,
            RestrictMode = false,
            LastLoginTime = DateTime.UtcNow,
            UsernameLastChange = DateTime.UtcNow
        };
        db.Insert(newUser);
        newUser.Id = log.AddResultAndTransform(
                db.SingleOrDefault<long>("SELECT id FROM bbl_user WHERE username = @0", newUser.Username)).OkOrDefault();
        db.Execute("INSERT INTO bbl_user_stats (uid) VALUES ((SELECT id FROM bbl_user WHERE username = @0))",
            newUser.Username);

        return Ok(new WebLoginRes { Work = true });
    }

    [HttpGet("/api/weblogintoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WebLoginTokenRes))]
    public ActionResult WebLoginToken() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        var res = new WebLoginTokenRes {
            Token = Guid.NewGuid(),
            MathValue1 = Random.Next(1, 50),
            MathValue2 = Random.Next(1, 50)
        };
        TokenDic[res.Token] = (res, DateTime.UtcNow + TimeSpan.FromMinutes(5));
        return Ok(res);
    }

    [HttpGet("/api/webupdateCookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<UpdateCookieInfo>))]
    public IActionResult WebUpdateCookie() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        var response = log.AddResultAndTransform(this.LoginTokenInfo(db)).OkOr(Option<TokenInfo>.Empty);
        if (response.IsSet() == false) {
            return Ok(ApiTypes.ExistOrFoundInfo<UpdateCookieInfo>.NotExist());
        }

        var f = log.AddResultAndTransform(this.GetCookieToken()).OkOr(Option<Guid>.Empty);
        if (f.IsSet() == false)
            return BadRequest();
        this.AppendCookie(ECookie.LoginCookie, f.Unwrap().ToString());

        var dbResp = log.AddResultAndTransform(db.Single<Entities.BblUser>(@$"
SELECT email, username FROM bbl_user
WHERE id = {response.Unwrap().UserId}
"));
        if (dbResp == EResult.Err) {
            return Ok(ApiTypes.ExistOrFoundInfo<UpdateCookieInfo>.NotExist());
        }

        return Ok(new ApiTypes.ExistOrFoundInfo<UpdateCookieInfo> {
            ExistOrFound = true, Value = new UpdateCookieInfo() {
                Email = dbResp.Ok().Email,
                Username = dbResp.Ok().Username
            }
        });
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResetPasswdAndSendEmailRes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResetPasswdAndSendEmailRes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult ResetPasswdAndSendEmail([FromBody] ResetPasswdAndSendEmailProp prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.AnyValidate() == EResult.Err) {
            return Ok(new ResetPasswdAndSendEmailRes { Work = false, TimeOut = false });
        }
        
        Option<IPAddress> optionIpAddress = Option<IPAddress>.Trim(log.AddResultAndTransform(GetIpAddress()));
        if (optionIpAddress.IsSet() == false)
            return BadRequest("IP IS NEEDED");
        var ipAddress = optionIpAddress.Unwrap();
        
        FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

        if (CallsForResetPasswd.TryGetValue(ipAddress, out var lastCall)) {
            if (lastCall.Calls > 3)
                return Ok(new ResetPasswdAndSendEmailRes { Work = false, TimeOut = true });
            CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
        }
        else {
            CallsForResetPasswd[ipAddress] = (lastCall.LastCall, lastCall.Calls + 1);
        }

        if (Email.ValidateEmail(prop.Email!) == false)
            return Ok(new ResetPasswdAndSendEmailRes { Work = false, TimeOut = false });

        var sql = (string.IsNullOrEmpty(prop.Email) == false) switch {
            true => new Sql(@"
SELECT username, id, email FROM bbl_user 
WHERE email = @0 
LIMIT 1", prop.Email),
            _ => new Sql(@"
SELECT username, id, email FROM bbl_user 
WHERE username = @0 
LIMIT 1", prop.Username)
        };
        
        var dbRes = log.AddResultAndTransform(db.SingleOrDefault<Entities.BblUser>(sql)).OkOrDefault();
        if (dbRes is null)
            return Ok(new ResetPasswdAndSendEmailRes { Work = false, TimeOut = false });

        var token = RandomText.NextAZ09(16);
        ResetPasswdTime[token] = (DateTime.UtcNow, dbRes.Id);

        SendEmail.MainSendResetEmail(dbRes.Id, dbRes.Username ?? "", dbRes.Email!, token);

        return Ok(new ResetPasswdAndSendEmailRes { Work = true, TimeOut = false });
    }

    [HttpPost("/api/token/newpasswdwithtoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(WebReplacePasswordWithToken))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SetNewPasswd([FromBody] ApiTypes.Api2GroundNoHeader<SetNewPasswdProp> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false) {
            return Ok(new WebReplacePasswordWithToken {
                Work = false,
                ErrorMsg = "Send Error"
            });
        }

        FilterOldValuesFromCallsForResetPasswdAndResetPasswdTime();

        var body = prop.Body!;

        if (ResetPasswdTime.TryGetValue(body.Token!, out var tokenValue) == false
            || tokenValue.UserId != body.UserId)
            return Ok(new WebReplacePasswordWithToken {
                Work = false,
                ErrorMsg = "Token To Old Or User Not Exist"
            });

        if (body.NewPasswd is null || body.NewPasswd.Length < 6)
            return Ok(new WebReplacePasswordWithToken {
                Work = false,
                ErrorMsg = "Password To Short"
            });
        
        var response = log.AddResultAndTransform(db.Execute(@$"
UPDATE bbl_user 
SET password = @0
WHERE id = {tokenValue.UserId}
", this.ToPasswdHash(body.NewPasswd)));

        if (response == EResult.Err) {
            return Ok(new WebReplacePasswordWithToken {
                Work = false,
                ErrorMsg = "Server Error"
            });
        }

        return Ok(new WebReplacePasswordWithToken {
            Work = true,
            ErrorMsg = ""
        });
    }

    [HttpPost("/api/signin/patreon")]
    public async Task<IActionResult> PatreonSignLogin([FromForm] string provider) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
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

    [HttpGet("/api/signout/patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.Work))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult PatreonSignout() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        // TODO PatreonSignout


        // Instruct the cookies middleware to delete the local cookie created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        return Ok();
    }


    [HttpGet("/api/weblogout")]
    public IActionResult RemoveCookie() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        RemoveCookieByEName(ECookie.LoginCookie);
        return Ok();
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
    public sealed class ResetPasswdAndSendEmailRes {
        public bool Work { get; set; }
        public bool TimeOut { get; set; }
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebLoginWithUsernameProp : ValidateAll, IValidatePasswd, IValidateUsername {
        public int Math { get; set; }
        public Guid Token { get; set; }
        public string? Passwd { get; set; }
        public string? Username { get; set; }
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UpdateCookieInfo {
        public string? Username { get; set; }
        public string? Email { get; set; }
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
    
    public sealed class WebLoginTokenRes {
        public Guid Token { get; set; }
        public int MathValue1 { get; set; }
        public int MathValue2 { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebLoginProp : ValidateAll, IValidatePasswd, IValidateEmail {
        public int Math { get; set; }
        public Guid Token { get; set; }
        public string? Email { get; set; }
        public string? Passwd { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    private sealed class WebLoginRes {
        public bool Work { get; set; }
        public bool UserOrPasswdOrMathIsFalse { get; set; }
        public bool UsernameExist { get; set; }
        public bool EmailExist { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class WebReplacePasswordWithToken {
        public bool Work { get; set; }
        public string? ErrorMsg { get; set; }
    }
}