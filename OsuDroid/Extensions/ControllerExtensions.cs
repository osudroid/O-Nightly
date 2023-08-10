using System.Net;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.Lib;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using EModelResult = OsuDroidAttachment.Class.EModelResult;

#pragma warning disable CA2252

namespace OsuDroid.Extensions;

public abstract class ControllerExtensions : ControllerBase {
    public enum ECookie {
        LoginCookie
    }

    public IActionResult TransactionToIResult<T>(Transaction<ApiTypes.ViewExistOrFoundInfo<T>> transaction) {
        return transaction.Result switch {
            EModelResult.Ok => Ok(transaction.OptionResponse.Unwrap()),
            EModelResult.BadRequest => BadRequest(),
            EModelResult.InternalServerError => InternalServerError(),
            EModelResult.BadRequestWithMessage => BadRequest(transaction.OptionResponse.Unwrap()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public IActionResult TransactionToIResult(Transaction<IActionResult> transaction) {
        return transaction.Result switch {
            EModelResult.Ok => transaction.OptionResponse.Unwrap(),
            EModelResult.BadRequest => BadRequest(),
            EModelResult.InternalServerError => InternalServerError(),
            EModelResult.BadRequestWithMessage => transaction.OptionResponse.Unwrap(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public IActionResult TransactionToIResult<IActionResult>(IActionResult actionResult) {
        return actionResult;
    }

    public IActionResult TransactionToIResult(Transaction<ApiTypes.ViewWork> transaction) {
        return transaction.Result switch {
            EModelResult.Ok => Ok(transaction.OptionResponse.Unwrap()),
            EModelResult.BadRequest => BadRequest(),
            EModelResult.InternalServerError => InternalServerError(),
            EModelResult.BadRequestWithMessage => BadRequest(transaction.OptionResponse.Unwrap()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string FixUsername(string username) {
        return username.Trim();
    }

    public IActionResult InternalServerError() {
        return new StatusCodeResult(500);
    }

    public async Task<IActionResult> RollbackAndGetBadRequestAsync(NpgsqlTransaction dbT) {
        await dbT.RollbackAsync();
        return BadRequest();
    }

    public async Task<IActionResult> RollbackAndGetBadRequestAsync(NpgsqlTransaction dbT, string msg) {
        await dbT.RollbackAsync();
        return BadRequest(msg);
    }

    public async Task<IActionResult> RollbackAndGetNotFound(NpgsqlTransaction dbT) {
        await dbT.RollbackAsync();
        return NotFound();
    }

    public async Task<IActionResult> RollbackAndGetNotFound(NpgsqlTransaction dbT, string message) {
        await dbT.RollbackAsync();
        return NotFound(message);
    }

    public Result<Option<UserIdAndToken>, string> LoginTokenInfo(NpgsqlConnection db) {
        if (!HttpContext.Items.TryGetValue(PrivilegeMiddleware.ItemName, out var data))
            return Result<Option<UserIdAndToken>, string>.Ok(Option<UserIdAndToken>.Empty);

        if (data is not UserIdAndToken userIdAndToken)
            return Result<Option<UserIdAndToken>, string>.Err(TraceMsg.WithMessage("data is not UserIdAndToken"));

        return Result<Option<UserIdAndToken>, string>.Ok(Option<UserIdAndToken>.With(userIdAndToken));
    }

    public Result<Option<Guid>, string> GetCookieToken() {
        var cookies = GetCookies();
        if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
            return Result<Option<Guid>, string>.Ok(Option<Guid>.Empty);

        if (Guid.TryParse(cookie, out var guid) == false)
            return Result<Option<Guid>, string>.Err("cookie Is Not Valid Guid");

        return Result<Option<Guid>, string>.Ok(Option<Guid>.With(guid));
    }
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

    internal Result<Option<IPAddress>, string> GetIpAddress() {
        try {
            if (!Request.Headers.TryGetValue("X-Forwarded-For", out var ip) || ip.Count == 0)
                return Result<Option<IPAddress>, string>.Ok(Option<IPAddress>.Empty);
            return Result<Option<IPAddress>, string>
                .Ok(Option<IPAddress>.With(IPAddress.Parse(ip.FirstOrDefault()!.Split(",")[0])));
        }
        catch (Exception e) {
            return Result<Option<IPAddress>, string>.Err(e.ToString());
        }
    }

    public Option<ECookie> NameToECookie(string name) {
        return name switch {
            "LoginCookie" => Option<ECookie>.With(ECookie.LoginCookie),
            _ => Option<ECookie>.Empty
        };
    }

    private Option<string> ECookieToString(ECookie eCookie) {
        return Option<string>.NullSplit(eCookie switch {
                ECookie.LoginCookie => "LoginCookie",
                _ => null
            }
        );
    }

    public Dictionary<ECookie, string> GetCookies() {
        try {
            var request = Request;
            var stringValues = request.Headers["Cookie"];
            if (stringValues.Count == 0)
                return new Dictionary<ECookie, string>(0);

            var res = new Dictionary<ECookie, string>(10);

            foreach (var i in stringValues) {
                var nameAndValue = (i ?? "").Split("=", StringSplitOptions.TrimEntries);
                var eCookie = NameToECookie(nameAndValue[0]);

                if (eCookie.IsSet() == false) continue;

                res.Add(eCookie.Unwrap(), nameAndValue[1]);
            }

            return res;
        }
        catch (Exception) {
            return new Dictionary<ECookie, string>(0);
        }
    }

    public void AppendCookie(ECookie eCookie, string value) {
        var eCookieToString = ECookieToString(eCookie);

        if (eCookieToString.IsSet() == false)
            throw new NullReferenceException(nameof(eCookieToString));
        // TODO SET SITE NAME
        var cookie = eCookieToString.Unwrap();
        Response.Cookies.Append(cookie, value, new CookieOptions {
                Secure = false,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Domain = Setting.Domain_Name!.Value,
                MaxAge = new TimeSpan(TimeSpan.TicksPerDay * 30)
            }
        );
    }

    public ResultErr<string> RemoveCookieByEName(ECookie eCookie) {
        var cookieToString = ECookieToString(eCookie);
        if (cookieToString.IsSet() == false)
            return ResultErr<string>.Err("Can Not Convert Cookie To String");

        Response.Cookies.Delete(cookieToString.Unwrap(), new CookieOptions {
                Domain = Setting.Domain_Name!.Value,
                SameSite = SameSiteMode.Lax
            }
        );
        return ResultErr<string>.Ok();
    }

    public UserCookieControllerHandler ControllerHandlerBuild() {
        return new UserCookieControllerHandler(this);
    }
}