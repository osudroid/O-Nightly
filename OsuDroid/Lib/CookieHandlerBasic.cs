namespace OsuDroid.Lib; 

public class CookieHandlerBasic: ICookieHandler {
    public string Name { get; } = "BASIC";
    
    public Result<(bool IsOk, long UserId), string> HandleCookie(
        IRequestCookieCollection requestCookie, IResponseCookies responseCookies) {

        var cookieOption = GetCookie(requestCookie);
        if (cookieOption.IsSet() == false) {
            return Result<(bool IsOk, long UserId), string>.Ok((false, -1));
        }

        var s = cookieOption.Unwrap();
        // TODO Write
        return default;
    }
    
    public Option<string> GetCookie(IRequestCookieCollection requestCookie) {
        var request = requestCookie;

        return request.TryGetValue(LoginCookie, out var value) 
            ? new Option<string>(value) 
            : default;
    }
    
    private const string LoginCookie = "LoginCookie";
}