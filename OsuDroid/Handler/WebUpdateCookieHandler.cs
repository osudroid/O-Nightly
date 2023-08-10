using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Class;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class WebUpdateCookieHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper, OptionHandlerOutput<ViewUpdateCookieInfo>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewUpdateCookieInfo>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerWrapper request) {
        var db = dbWrapper.Db;

        var cookieOpt = request.Controller.GetCookie();
        if (cookieOpt.IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateCookieInfo>, string>.Ok(
                OptionHandlerOutput<ViewUpdateCookieInfo>.Empty
            );

        var tokenId = cookieOpt.Unwrap();

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var userIdAndTokenResult = await tokenHandler.GetTokenInfoAsync(db, cookieOpt.Unwrap());

        if (userIdAndTokenResult == EResult.Err)
            return userIdAndTokenResult.ChangeOkType<OptionHandlerOutput<ViewUpdateCookieInfo>>();

        if (userIdAndTokenResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateCookieInfo>, string>.Ok(
                OptionHandlerOutput<ViewUpdateCookieInfo>.Empty
            );

        var info = userIdAndTokenResult.Ok().Unwrap();

        var updateResult = await tokenHandler.SetOverwriteAsync(db, new TokenInfoWithGuid {
                Token = tokenId,
                TokenInfo = new TokenInfo { CreateDay = DateTime.UtcNow, UserId = info.UserId }
            }
        );

        if (updateResult == EResult.Err)
            return updateResult.ConvertTo<OptionHandlerOutput<ViewUpdateCookieInfo>>();

        return Result<OptionHandlerOutput<ViewUpdateCookieInfo>, string>.Ok(
            OptionHandlerOutput<ViewUpdateCookieInfo>.With(new ViewUpdateCookieInfo {
                    UserId = info.UserId
                }
            )
        );
    }
}