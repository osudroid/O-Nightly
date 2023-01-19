using System.Net;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Utils;

#pragma warning disable CA2252

namespace OsuDroid.Extensions;

public abstract class ControllerExtensions : ControllerBase {
    public enum ECookie {
        LoginCookie
    }

    public string FixUsername(string username) {
        return username.Trim();
    }

    public Response<TokenInfo> LoginTokenInfo(SavePoco db) {
        try {
            var cookies = GetCookies();
            if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
                return Response<TokenInfo>.Err;
            if (Guid.TryParse(cookie, out var guid) == false) return Response<TokenInfo>.Err;
            var resp = TokenHandlerManger
                .GetOrCreateCacheDatabase(ETokenHander.User)
                .GetTokenInfo(db, guid);
            if (resp == EResponse.Err) {
                RemoveCookieByEName(ECookie.LoginCookie);
                return resp;
            }

            return resp;
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception _) {
            return Response<TokenInfo>.Err;
        }
#endif
    }

    public Response<Guid> GetCookieToken() {
        var cookies = GetCookies();
        if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
            return Response<Guid>.Err;
        if (Guid.TryParse(cookie, out var guid) == false) return Response<Guid>.Err;
        return Response<Guid>.Ok(guid);
    }

    public Response LoginTokenInfoRefresh(SavePoco db) {
        try {
            var cookies = GetCookies();
            if (cookies.TryGetValue(ECookie.LoginCookie, out var cookie) == false)
                return LamLibAllOver.Response.Err();
            if (Guid.TryParse(cookie, out var guid) == false) return LamLibAllOver.Response.Err();
            var resp = TokenHandlerManger
                .GetOrCreateCacheDatabase(ETokenHander.User)
                .Refresh(db, guid);

            if (resp == EResponse.Err) {
                RemoveCookieByEName(ECookie.LoginCookie);
                return resp;
            }

            return resp;
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception _) {
            return LamLibAllOver.Response.Err();
        }
#endif
    }

    public string ToPasswdHash(string passwd) {
        return Password.Hash(passwd).OkOr("");
    }

    internal Response<IPAddress> GetIpAddress() {
        try {
            if (!Request.Headers.TryGetValue("X-Forwarded-For", out var ip) || ip.Count == 0)
                return Response<IPAddress>.Err;
            return Response<IPAddress>.Ok(IPAddress.Parse(ip.FirstOrDefault()!.Split(",")[0]));
        }
        catch (Exception) {
            return Response<IPAddress>.Err;
        }
    }

    public ECookie? NameToECookie(string name) {
        return name switch {
            "LoginCookie" => ECookie.LoginCookie,
            _ => null
        };
    }

    private string? ECookieToString(ECookie eCookie) {
        return eCookie switch {
            ECookie.LoginCookie => "LoginCookie",
            _ => null
        };
    }

    public Dictionary<ECookie, string> GetCookies() {
        try {
            var request = Request;
            var stringValues = request.Headers["Cookie"];
            if (stringValues.Count == 0) return new Dictionary<ECookie, string>(0);

            var res = new Dictionary<ECookie, string>(10);

            foreach (var i in stringValues) {
                var nameAndValue = (i ?? "").Split("=", StringSplitOptions.TrimEntries);
                var eCookie = NameToECookie(nameAndValue[0]);

                if (eCookie.HasValue == false) continue;

                res.Add(eCookie.Value, nameAndValue[1]);
            }

            return res;
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception) {
            return new Dictionary<ECookie, string>();
        }
#endif
    }

    public void AppendCookie(ECookie eCookie, string value) {
        var eCookieToString = ECookieToString(eCookie);

        if (eCookieToString is null)
            throw new NullReferenceException(nameof(eCookieToString));
        // TODO SET SITE NAME

        Response.Cookies.Append(eCookieToString, value, new CookieOptions {
            Secure = false,
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Domain = Env.Domain,
            MaxAge = new TimeSpan(TimeSpan.TicksPerDay * 30)
        });
    }

    public void RemoveCookieByEName(ECookie eCookie) {
        Response.Cookies.Delete(ECookieToString(eCookie)!, new CookieOptions {
            Domain = Env.Domain,
            SameSite = SameSiteMode.Lax
        });
    }

    /// <summary> StatusCodes.Status500InternalServerError </summary>
    public StatusCodeResult GetInternalServerError() {
        return new StatusCodeResult(500);
    }
}