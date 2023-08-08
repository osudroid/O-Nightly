using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Validation;

namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [PrivilegeRoute("/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfoByCookie() {
        var transaction = await new AttachmentServiceApiBuilder<
            NpgsqlCreates.DbWrapper,
            LogWrapper,
            UserCookieControllerHandler,
            UserCookieControllerHandler,
            OptionHandlerOutput<ViewUserInfo>, ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>> {
            DbCreates = new NpgsqlCreates(),
            LoggerCreates = new LogCreates(),
            ValidationHandler =
                new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, UserCookieControllerHandler>(),
            TransformHandler = new TransformParse<UserCookieControllerHandler>(),
            Handler = new GetUserInfoByCookieHandler(),
            OutputHandler = new ViewExistOrFoundInfoHandler<ViewUserInfo>()
        }.ToServiceAndRun(ControllerHandlerBuild());

        return TransactionToIResult(transaction);
    }
}