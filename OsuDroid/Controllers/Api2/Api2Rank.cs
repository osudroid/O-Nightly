using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;

namespace OsuDroid.Controllers.Api2 {
    public class Api2Rank : ControllerExtensions {
        [HttpPost("/api2/rank/map-file")]
        [PrivilegeRoute(route: "/api2/rank/map-file")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MapFileRank([FromBody] PostApi.PostApi2GroundWithHash<PostApi2MapFileRank> prop) {
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>, 
                ControllerPostWrapper<Api2MapFileRankDto>,
                OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>,
                ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new Api2MapFileRankValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>,
                    ControllerPostWrapper<Api2MapFileRankDto>>((i) 
                    => new ControllerPostWrapper<Api2MapFileRankDto>(i.Controller, DtoMapper.Api2MapFileRankToDto(i.Post.Body!))),
                handler: new Handler.MapFileRankHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<IReadOnlyList<ViewMapTopPlays>>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>(this.ControllerHandlerBuild(), prop)
            );
        
            return TransactionToIResult(transaction);
        }
    }
}