using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Lib;

public class CookieHandlerBasic : ICookieHandler {
    private const string LoginCookie = "LoginCookie";
    public string Name { get; } = "BASIC";

    public async Task<Result<(bool IsOk, long UserId, Guid Token), string>> HandleCookieAsync(
        NpgsqlConnection db, IRequestCookieCollection requestCookie, IResponseCookies responseCookies) {
        var cookieOption = GetCookie(requestCookie);
        if (cookieOption.IsSet() == false)
            return Result<(bool IsOk, long UserId, Guid Token), string>.Ok((false, -1, default));

        if (Guid.TryParse(cookieOption.Unwrap(), out var token) == false)
            return Result<(bool IsOk, long UserId, Guid Token), string>.Ok((false, -1, default));

        var manger = TokenHandlerManger.GetOrCreateCacheDatabase();
        var result = await manger.GetTokenInfoAsync(db, token);
        if (result == EResult.Err)
            return result.ChangeOkType<(bool IsOk, long UserId, Guid Token)>();
        if (result.Ok().IsNotSet())
            return Result<(bool IsOk, long UserId, Guid Token), string>.Ok((false, -1, default));

        var tokenInfo = result.Ok().Unwrap();

        if (tokenInfo.CreateDay.AddDays(1) < DateTime.UtcNow)
            await manger.SetOverwriteAsync(db, new TokenInfoWithGuid { Token = token, TokenInfo = tokenInfo });

        responseCookies.Append(
            Name,
            token.ToString(),
            new CookieOptions {
                Expires = new DateTimeOffset(DateTime.UtcNow, new TimeSpan(30, 0, 0, 0))
            });

        return Result<(bool IsOk, long UserId, Guid Token), string>.Ok((true, tokenInfo.UserId, token));
    }

    public Option<string> GetCookie(IRequestCookieCollection requestCookie) {
        var request = requestCookie;

        return request.TryGetValue(LoginCookie, out var value)
            ? new Option<string>(value)
            : default;
    }
}