using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.DbBuilder;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2; 

public class Api2Play : ControllerExtensions {
    [HttpPost("/api2/play/by-id")]
    [PrivilegeRoute("/api2/play/by-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlayInfoById>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
    public async Task<IActionResult> GetPlayById([FromBody] PostApi.PostApi2GroundWithHash<PostApi2PlayById> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new Api2PlayByIdValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2PlayById>>,
                ControllerPostWrapper<Api2PlayByIdDto>>(i
                => new ControllerPostWrapper<Api2PlayByIdDto>(i.Controller, DtoMapper.Api2PlayByIdToDto(i.Post.Body!))),
            new GetPlayByIdHandler(),
            new ViewExistOrFoundInfoHandler<ViewPlayInfoById>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2PlayById>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/play/recent")]
    [PrivilegeRoute("/api2/play/recent")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewPlayScoreWithUsername>>))]
    public async Task<IActionResult> GetRecentPlay([FromBody] PostApi.PostApi2GroundNoHeader<PostRecentPlays> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new RecentPlaysValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostRecentPlays>>,
                ControllerPostWrapper<RecentPlaysDto>>(i
                => new ControllerPostWrapper<RecentPlaysDto>(i.Controller, DtoMapper.RecentPlaysToDto(i.Post.Body!))),
            new GetRecentPlayHandler(),
            new ViewExistOrFoundInfoHandler<IReadOnlyList<ViewPlayScoreWithUsername>>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostRecentPlays>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}