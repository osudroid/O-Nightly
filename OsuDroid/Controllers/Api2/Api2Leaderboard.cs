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

public class Api2Leaderboard : ControllerExtensions {
    [HttpPost("/api2/leaderboard")]
    [PrivilegeRoute("/api2/leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderBoard([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoard> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new LeaderBoardValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoard>>,
                ControllerPostWrapper<LeaderBoardDto>>(i
                => new ControllerPostWrapper<LeaderBoardDto>(i.Controller, DtoMapper.LeaderBoardToDto(i.Post.Body!))),
            new GetLeaderBoardHandler(),
            new ViewExistOrFoundInfoHandler<List<ViewLeaderBoardUser>>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoard>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/leaderboard/user")]
    [PrivilegeRoute("/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new LeaderBoardUserValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser>>,
                ControllerPostWrapper<LeaderBoardUserDto>>(i
                => new ControllerPostWrapper<LeaderBoardUserDto>(i.Controller,
                    DtoMapper.LeaderBoardUserToDto(i.Post.Body!))),
            new GetUserLeaderBoardRankHandler(),
            new ViewExistOrFoundInfoHandler<ViewLeaderBoardUser>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser>>(ControllerHandlerBuild(),
                prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/leaderboard/search-user")]
    [PrivilegeRoute("/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new LeaderBoardSearchUserValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser>>,
                ControllerPostWrapper<LeaderBoardSearchUserDto>>(i
                => new ControllerPostWrapper<LeaderBoardSearchUserDto>(i.Controller,
                    DtoMapper.LeaderBoardSearchUserToDto(i.Post.Body!))),
            new GetUserLeaderBoardRankSearchUser(),
            new ViewExistOrFoundInfoHandler<List<ViewLeaderBoardUser>>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser>>(
                ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}