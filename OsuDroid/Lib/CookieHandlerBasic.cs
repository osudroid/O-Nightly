using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Lib;

public class CookieHandlerBasic : ICookieHandler {
    private const string LoginCookie = "LoginCookie";
    public string Name { get; } = "BASE";

    public async Task<Result<(bool IsOk, long UserId, Guid Token), string>> HandleCookieAsync(
        NpgsqlConnection db,
        IRequestCookieCollection requestCookie,
        IResponseCookies responseCookies) {
        try
        {
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
                LoginCookie,
                token.ToString(),
                new CookieOptions {
                    Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30))
                }
            );

            return Result<(bool IsOk, long UserId, Guid Token), string>.Ok((true, tokenInfo.UserId, token));
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return Result<(bool IsOk, long UserId, Guid Token), string>.Err(e.ToString());
        }
    }

    public Option<string> GetCookie(IRequestCookieCollection requestCookie) {
        var request = requestCookie;

        return request.TryGetValue(LoginCookie, out var value)
            ? new Option<string>(value)
            : default;
    }
}