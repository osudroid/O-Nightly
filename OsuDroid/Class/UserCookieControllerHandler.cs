using System.Net;
using Npgsql;
using OsuDroid.Extensions;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Class;

public class UserCookieControllerHandler(ControllerExtensions controller) : ITransformOutput, IInput {
    private ControllerExtensions Controller { get; } = controller;

    public Option<Guid> GetCookie() {
        return controller.GetCookieToken().Unwrap() switch {
            { Status: EResult.Err } => Option<Guid>.Empty,
            { Status: EResult.Ok } a => a.Ok,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public ResultErr<string> SetCookie(Guid cookie) {
        var cookies = controller.GetCookies();
        cookies[ControllerExtensions.ECookie.LoginCookie] = cookie.ToString();
        return ResultErr<string>.Ok();
    }

    public Option<(Guid Cookie, long UserId)> GetCookieAndUserId(NpgsqlConnection db) {
        var v = controller.LoginTokenInfo(db);

        if (v == EResult.Err) throw new Exception(v.Err());

        var option = v.Ok();
        return option.IsNotSet()
            ? Option<(Guid Cookie, long UserId)>.Empty
            : Option<(Guid Cookie, long UserId)>.With((option.Unwrap().Token, option.Unwrap().UserId));
    }

    public Result<Option<IPAddress>, string> GetIpAddress() {
        return Controller.GetIpAddress();
    }

    public void RemoveCookieByEName(ControllerExtensions.ECookie loginCookie) {
        Controller.RemoveCookieByEName(loginCookie);
    }
}