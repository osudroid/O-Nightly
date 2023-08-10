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

namespace OsuDroid.Controllers.Api2;

public class Api2Rank : ControllerExtensions {
    [HttpPost("/api2/rank/map-file")]
    [PrivilegeRoute("/api2/rank/map-file")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>)
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MapFileRank([FromBody] PostApi.PostApi2GroundWithHash<PostApi2MapFileRank> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new Api2MapFileRankValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>,
                ControllerPostWrapper<Api2MapFileRankDto>>(i
                => new ControllerPostWrapper<Api2MapFileRankDto>(i.Controller,
                    DtoMapper.Api2MapFileRankToDto(i.Post.Body!)
                )
            ),
            new MapFileRankHandler(),
            new ViewExistOrFoundInfoHandler<IReadOnlyList<ViewMapTopPlays>>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>(ControllerHandlerBuild(),
                prop
            )
        );

        return TransactionToIResult(transaction);
    }
}