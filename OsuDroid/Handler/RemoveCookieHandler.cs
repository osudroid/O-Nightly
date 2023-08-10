using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class RemoveCookieHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper, OptionHandlerOutput<ApiTypes.ViewWork>> {
    public async ValueTask<Result<OptionHandlerOutput<ApiTypes.ViewWork>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerWrapper request) {
        var db = dbWrapper.Db;
        var controller = request.Controller;

        var cookieOption = controller.GetCookie();
        if (cookieOption.IsNotSet())
            return Result<OptionHandlerOutput<ApiTypes.ViewWork>, string>.Ok(
                OptionHandlerOutput<ApiTypes.ViewWork>.With(new ApiTypes.ViewWork {
                        HasWork = true
                    }
                )
            );

        var cookie = cookieOption.Unwrap();

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        await tokenHandler.RemoveTokenAsync(db, cookie);

        controller.RemoveCookieByEName(ControllerExtensions.ECookie.LoginCookie);
        return Result<OptionHandlerOutput<ApiTypes.ViewWork>, string>
            .Ok(OptionHandlerOutput<ApiTypes.ViewWork>.With(new ApiTypes.ViewWork { HasWork = true }));
    }
}