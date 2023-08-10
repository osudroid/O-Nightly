using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Validation;

namespace OsuDroid.Controllers.Api2;

public class Api2Statistic : ControllerExtensions {
    [HttpGet("/api2/statistic/active-user")]
    [PrivilegeRoute("/api2/statistic/active-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>)
    )]
    public async Task<IActionResult> GetActiveUser() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformAction<
                ControllerWrapper,
                ControllerWrapper>(i
                => new ControllerWrapper(i.Controller)
            ),
            new ActiveUserHandler(),
            new ViewExistOrFoundInfoHandler<ViewStatisticActiveUser>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }

    [HttpGet("/api2/statistic/all-patreon")]
    [PrivilegeRoute("/api2/statistic/all-patreon")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>)
    )]
    public async Task<IActionResult> GetAllPatreon() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformAction<
                ControllerWrapper,
                ControllerWrapper>(i
                => new ControllerWrapper(i.Controller)
            ),
            new GetAllPatreonHandler(),
            new ViewExistOrFoundInfoHandler<List<ViewUsernameAndId>>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }
}