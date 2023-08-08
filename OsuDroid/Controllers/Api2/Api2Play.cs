using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2 {
    public class Api2Play : ControllerExtensions {
        [HttpPost("/api2/play/by-id")]
        [PrivilegeRoute(route: "/api2/play/by-id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlayInfoById>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPlayById([FromBody] PostApi.PostApi2GroundWithHash<PostApi2PlayById> prop) {
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2PlayById>>, 
                ControllerPostWrapper<Api2PlayByIdDto>, 
                OptionHandlerOutput<ViewPlayInfoById>, 
                ApiTypes.ViewExistOrFoundInfo<ViewPlayInfoById>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new Api2PlayByIdValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2PlayById>>,
                    ControllerPostWrapper<Api2PlayByIdDto>>((i) 
                    => new ControllerPostWrapper<Api2PlayByIdDto>(i.Controller, DtoMapper.Api2PlayByIdToDto(i.Post.Body!))),
                handler: new Handler.GetPlayByIdHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlayInfoById>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2PlayById>>(this.ControllerHandlerBuild(), prop)
            );
            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/play/recent")]
        [PrivilegeRoute(route: "/api2/play/recent")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>>))]
        public async Task<IActionResult> GetRecentPlay([FromBody] PostApi.PostApi2GroundNoHeader<PostRecentPlays> prop) {
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostRecentPlays>>, 
                ControllerPostWrapper<RecentPlaysDto>, 
                OptionHandlerOutput<IReadOnlyList<ViewPlayScoreWithUsername>>, 
                ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new RecentPlaysValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostRecentPlays>>,
                    ControllerPostWrapper<RecentPlaysDto>>((i) 
                    => new ControllerPostWrapper<RecentPlaysDto>(i.Controller, DtoMapper.RecentPlaysToDto(i.Post.Body!))),
                handler: new Handler.GetRecentPlayHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<IReadOnlyList<ViewPlayScoreWithUsername>>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostRecentPlays>>(this.ControllerHandlerBuild(), prop)
            );
            return TransactionToIResult(transaction);
        }
    }
}