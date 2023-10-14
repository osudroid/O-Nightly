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

public class Api2Admin : ControllerExtensions {
    [HttpGet("/api2/admin/isAdmin")]
    [PrivilegeRoute("/api2/admin/isAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IsAdmin() {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
            new TransformAction<
                ControllerWrapper,
                ControllerWrapper>(i
                => new ControllerWrapper(i.Controller)
            ),
            new IsAdminHandler(),
            new ViewExistOrFoundInfoHandler<ViewIsAdmin>(),
            new ControllerWrapper(ControllerHandlerBuild())
        );

        return TransactionToIResult(transaction);
    }
    
    // public IResult GetWidgetDataDbStats() {
    //     
    // }
    //
    // public IResult GetWidgetDataNewUser() {
    //     
    // }
    //
    // public IResult GetWidgetDataPreSummits() {
    //     
    // }
    //
    // public IResult GetWidgetDataSearchUser() {
    //     
    // }
    //
    // public IResult GetWidgetDataLogs() {
    //     
    // } 
}