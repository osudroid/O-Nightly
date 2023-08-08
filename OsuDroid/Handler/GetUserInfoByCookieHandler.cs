using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class GetUserInfoByCookieHandler :
    IHandler<NpgsqlCreates.DbWrapper, LogWrapper, UserCookieControllerHandler, OptionHandlerOutput<ViewUserInfo>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewUserInfo>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, UserCookieControllerHandler request) {
        var db = dbWrapper.Db;

        var cookieOpt = request.GetCookie();
        if (cookieOpt.IsNotSet())
            return Result<OptionHandlerOutput<ViewUserInfo>, string>.Ok(OptionHandlerOutput<ViewUserInfo>.Empty);

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var userIdAndTokenResult = await tokenHandler.GetTokenInfoAsync(db, cookieOpt.Unwrap());


        if (userIdAndTokenResult == EResult.Err)
            return userIdAndTokenResult.ChangeOkType<OptionHandlerOutput<ViewUserInfo>>();

        if (userIdAndTokenResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewUserInfo>, string>.Ok(OptionHandlerOutput<ViewUserInfo>.Empty);

        var result = await UserInfoManager
            .GetByUserIdAsync(db, userIdAndTokenResult.Ok().Unwrap().UserId);
        if (result == EResult.Err)
            return result.ChangeOkType<OptionHandlerOutput<ViewUserInfo>>();

        if (result.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewUserInfo>, string>
                .Err($"User With Id Not Found {userIdAndTokenResult.Ok().Unwrap().UserId}");

        var userInfo = result.Ok().Unwrap();

        return Result<OptionHandlerOutput<ViewUserInfo>, string>.Ok(OptionHandlerOutput<ViewUserInfo>.With(
            new ViewUserInfo {
                Id = userInfo.UserId,
                Username = userInfo.Username ?? "",
                Email = userInfo.Email ?? "",
                Region = userInfo.Region ?? "",
                Active = userInfo.Active,
                Banned = userInfo.Banned,
                RestrictMode = userInfo.RestrictMode,
                RegistTime = userInfo.RegisterTime,
                Supporter = userInfo.PatronEmailAccept
            }));
    }
}