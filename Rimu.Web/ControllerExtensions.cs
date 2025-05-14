using System.Net;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Environment.Adapter.Interface;

#pragma warning disable CA2252

namespace Rimu.Web;

public abstract class ControllerExtensions : ControllerBase {
    protected readonly IEnvJson EnvJson;
    protected readonly IEnvDb EnvDb;

    protected ControllerExtensions(IEnvJson envJson, IEnvDb envDb) {
        EnvJson = envJson;
        EnvDb = envDb;
    }

    public enum ECookie {
        LoginCookie
    }
   
    
    // public IActionResult TransactionToIResult(Transaction<IActionResult> transaction) {
    //     return transaction.Result switch {
    //         EModelResult.Ok => transaction.OptionResponse.Unwrap(),
    //         EModelResult.BadRequest => BadRequest(),
    //         EModelResult.InternalServerError => InternalServerError(),
    //         EModelResult.BadRequestWithMessage => transaction.OptionResponse.Unwrap(),
    //         _ => throw new ArgumentOutOfRangeException()
    //     };
    // }
    //
    // public IActionResult TransactionToIResult(Transaction<ApiTypes.ViewWork> transaction) {
    //     return transaction.Result switch {
    //         EModelResult.Ok => Ok(transaction.OptionResponse.Unwrap()),
    //         EModelResult.BadRequest => BadRequest(),
    //         EModelResult.InternalServerError => InternalServerError(),
    //         EModelResult.BadRequestWithMessage => BadRequest(transaction.OptionResponse.Unwrap()),
    //         _ => throw new ArgumentOutOfRangeException()
    //     };
    // }
    //
    // public string FixUsername(string username) {
    //     return username.Trim();
    // }
    //
    public IActionResult InternalServerError() {
        return new StatusCodeResult(500);
    }
    //
    // public async Task<IActionResult> RollbackAndGetBadRequestAsync(NpgsqlTransaction dbT) {
    //     await dbT.RollbackAsync();
    //     return BadRequest();
    // }
    //
    // public async Task<IActionResult> RollbackAndGetBadRequestAsync(NpgsqlTransaction dbT, string msg) {
    //     await dbT.RollbackAsync();
    //     return BadRequest(msg);
    // }
    //
    // public async Task<IActionResult> RollbackAndGetNotFound(NpgsqlTransaction dbT) {
    //     await dbT.RollbackAsync();
    //     return NotFound();
    // }
    //
    // public async Task<IActionResult> RollbackAndGetNotFound(NpgsqlTransaction dbT, string message) {
    //     await dbT.RollbackAsync();
    //     return NotFound(message);
    // }
    //
    // public SResult<Option<UserIdAndToken>> LoginTokenInfo(NpgsqlConnection db) {
    //     if (!HttpContext.Items.TryGetValue(PrivilegeMiddleware.ItemName, out var data))
    //         return SResult<Option<UserIdAndToken>>.Ok(Option<UserIdAndToken>.Empty);
    //
    //     if (data is not UserIdAndToken userIdAndToken)
    //         return SResult<Option<UserIdAndToken>>.Err(TraceMsg.WithMessage("data is not UserIdAndToken"));
    //
    //     return SResult<Option<UserIdAndToken>>.Ok(Option<UserIdAndToken>.With(userIdAndToken));
    // }
    //
    // public SResult<Option<Guid>> GetCookieToken() {
    //     var cookies = GetCookies();
    //     if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
    //         return SResult<Option<Guid>>.Ok(Option<Guid>.Empty);
    //
    //     if (Guid.TryParse(cookie, out var guid) == false)
    //         return SResult<Option<Guid>>.Err("cookie Is Not Valid Guid");
    //
    //     return SResult<Option<Guid>>.Ok(Option<Guid>.With(guid));
    // }
    //
    // public ResultErr<string> LoginTokenRefreshTime(SavePoco db) {
    //     try {
    //         var cookies = GetCookies();
    //         if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
    //             return ResultErr<string>.Err("LoginToken In Cookies Not Found");
    //         if (Guid.TryParse(cookie, out var guid) == false)
    //             return ResultErr<string>.Err("cookie Is Not Valid Guid");
    //         var resp = TokenHandlerManger
    //             .GetOrCreateCacheDatabase(ETokenHander.User)
    //             .RefreshAsync(db, guid);
    //         
    //         if (resp == EResult.Err) {
    //             RemoveCookieByEName(ECookie.LoginCookie);
    //         }
    //         
    //         return ResultErr<string>.Ok();
    //     }
    //     catch (Exception e) {
    //         return ResultErr<string>.Err(e.ToString());
    //     }
    // }

    internal SResult<Option<IPAddress>> GetIpAddress() {
        try {
            if (!Request.Headers.TryGetValue("X-Forwarded-For", out var ip) || ip.Count == 0)
                return SResult<Option<IPAddress>>.Ok(Option<IPAddress>.Empty);
            return SResult<Option<IPAddress>>
                .Ok(Option<IPAddress>.With(IPAddress.Parse(ip.FirstOrDefault()!.Split(",")[0])));
        }
        catch (Exception e) {
            return SResult<Option<IPAddress>>.Err(e.ToString());
        }
    }
    //
    // public Option<ECookie> NameToECookie(string name) {
    //     return name switch {
    //         "LoginCookie" => Option<ECookie>.With(ECookie.LoginCookie),
    //         _ => Option<ECookie>.Empty
    //     };
    // }
    //
    // private Option<string> ECookieToString(ECookie eCookie) {
    //     return Option<string>.NullSplit(eCookie switch {
    //             ECookie.LoginCookie => "LoginCookie",
    //             _ => null
    //         }
    //     );
    // }
    //
    // public Dictionary<ECookie, string> GetCookies() {
    //     try {
    //         var request = Request;
    //         var cookieHeader = request.Headers["Cookie"];
    //         if (cookieHeader.Count == 0) return new Dictionary<ECookie, string>(0);
    //
    //         var stringValues = cookieHeader[0].Trim().Split(";");
    //         if (stringValues.Length == 0)
    //             return new Dictionary<ECookie, string>(0);
    //
    //         var res = new Dictionary<ECookie, string>(2);
    //
    //         foreach (var i in stringValues) {
    //             var nameAndValue = (i ?? "").Split("=", StringSplitOptions.TrimEntries);
    //             var eCookie = NameToECookie(nameAndValue[0]);
    //
    //             if (eCookie.IsSet() == false) continue;
    //
    //             res.TryAdd(eCookie.Unwrap(), nameAndValue[1]);
    //         }
    //
    //         return res;
    //     }
    //     catch (Exception) {
    //         return new Dictionary<ECookie, string>(0);
    //     }
    // }
    //
    // public void AppendCookie(ECookie eCookie, string value) {
    //     var eCookieToString = ECookieToString(eCookie);
    //
    //     if (eCookieToString.IsSet() == false)
    //         throw new NullReferenceException(nameof(eCookieToString));
    //     // TODO SET SITE NAME
    //     var cookie = eCookieToString.Unwrap();
    //     
    //     Response.Cookies.Append(cookie, value, new CookieOptions {
    //             Secure = false,
    //             HttpOnly = false,
    //             SameSite = SameSiteMode.Lax,
    //             Domain = this.SettingsProvider.GetSettingsDb().Domain_Name,
    //             MaxAge = new TimeSpan(TimeSpan.TicksPerDay * 30)
    //         }
    //     );
    // }
    //
    // public ResultErr<string> RemoveCookieByEName(ECookie eCookie) {
    //     var cookieToString = ECookieToString(eCookie);
    //     if (cookieToString.IsSet() == false)
    //         return ResultErr<string>.Err("Can Not Convert Cookie To String");
    //
    //     Response.Cookies.Delete(cookieToString.Unwrap(), new CookieOptions {
    //             Domain = this.SettingsProvider.GetSettingsDb().Domain_Name,
    //             SameSite = SameSiteMode.Lax
    //         }
    //     );
    //     return ResultErr<string>.Ok();
    // }
    //
    // public UserCookieControllerHandler ControllerHandlerBuild() {
    //     return new UserCookieControllerHandler(this);
    // }
}