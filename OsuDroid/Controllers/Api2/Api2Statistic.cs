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

namespace OsuDroid.Controllers.Api2 {
    public class Api2Statistic : ControllerExtensions {
        [HttpGet("/api2/statistic/active-user")]
        [PrivilegeRoute(route: "/api2/statistic/active-user")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>))]
        public async Task<IActionResult> GetActiveUser() {
        
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerWrapper, 
                ControllerWrapper,
                OptionHandlerOutput<ViewStatisticActiveUser>,
                ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
                transformHandler: new TransformAction<
                    ControllerWrapper,
                    ControllerWrapper>((i) 
                    => new ControllerWrapper(i.Controller)),
                handler: new Handler.ActiveUserHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewStatisticActiveUser>(),
                input: new ControllerWrapper(this.ControllerHandlerBuild())
            );
        
            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/statistic/all-patreon")]
        [PrivilegeRoute(route: "/api2/statistic/all-patreon")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>))]
        public async Task<IActionResult> GetAllPatreon() {
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerWrapper, 
                ControllerWrapper,
                OptionHandlerOutput<List<ViewUsernameAndId>>,
                ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new ValidationHandlerNothing<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper>(),
                transformHandler: new TransformAction<
                    ControllerWrapper,
                    ControllerWrapper>((i) 
                    => new ControllerWrapper(i.Controller)),
                handler: new Handler.GetAllPatreonHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<List<ViewUsernameAndId>>(),
                input: new ControllerWrapper(this.ControllerHandlerBuild())
            );
        
            return TransactionToIResult(transaction);
        }
    }
}