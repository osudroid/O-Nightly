using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Extensions;
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
    [PrivilegeRoute(route: "/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfoByCookie() {
        Transaction<ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>> transaction = await new AttachmentServiceApiBuilder<
            OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
            Class.LogWrapper, 
            UserCookieControllerHandler, 
            UserCookieControllerHandler, 
            OptionHandlerOutput<ViewUserInfo>, 
            ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>> {
            
            DbCreates = new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            LoggerCreates = new Class.LogCreates(),
            ValidationHandler = new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, UserCookieControllerHandler>(),
            TransformHandler = new TransformParse<UserCookieControllerHandler>(),
            Handler = new Handler.GetUserInfoByCookieHandler(),
            OutputHandler = new ViewExistOrFoundInfoHandler<ViewUserInfo>(),
        }.ToServiceAndRun(this.ControllerHandlerBuild());

        return TransactionToIResult(transaction);
    }
}