using LamLogger;
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
using OsuDroidLib;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Lib;
using EModelResult = OsuDroid.Class.EModelResult;

namespace OsuDroid.Controllers.Api2;

public class Api2Leaderboard : ControllerExtensions {
    [HttpPost("/api2/leaderboard")]
    [PrivilegeRoute(route: "/api2/leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderBoard([FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoard> prop) {
        var transaction = await Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new LeaderBoardValidation(),
            transformHandler: new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoard>>,
                ControllerPostWrapper<LeaderBoardDto>>((i) 
                => new ControllerPostWrapper<LeaderBoardDto>(i.Controller, DtoMapper.LeaderBoardToDto(i.Post.Body!))),
            handler: new Handler.GetLeaderBoardHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<List<ViewLeaderBoardUser>>(),
            input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoard>>(this.ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/leaderboard/user")]
    [PrivilegeRoute(route: "/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser> prop) {
        
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
            OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
            Class.LogWrapper, 
            ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser>>, 
            ControllerPostWrapper<LeaderBoardUserDto>, 
            OptionHandlerOutput<ViewLeaderBoardUser>, 
            ApiTypes.ViewExistOrFoundInfo<ViewLeaderBoardUser>>(
            
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new LeaderBoardUserValidation(),
            transformHandler: new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser>>,
                ControllerPostWrapper<LeaderBoardUserDto>>((i) 
                => new ControllerPostWrapper<LeaderBoardUserDto>(i.Controller, DtoMapper.LeaderBoardUserToDto(i.Post.Body!))),
            handler: new Handler.GetUserLeaderBoardRankHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewLeaderBoardUser>(),
            input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardUser>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/leaderboard/search-user")]
    [PrivilegeRoute(route: "/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserLeaderBoardRank(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser> prop) {
        
        
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
            OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
            Class.LogWrapper, 
            ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser>>, 
            ControllerPostWrapper<LeaderBoardSearchUserDto>, 
            OptionHandlerOutput<List<ViewLeaderBoardUser>>, 
            ApiTypes.ViewExistOrFoundInfo<List<ViewLeaderBoardUser>>>(
            
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new LeaderBoardSearchUserValidation(),
            transformHandler: new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser>>,
                ControllerPostWrapper<LeaderBoardSearchUserDto>>((i) 
                => new ControllerPostWrapper<LeaderBoardSearchUserDto>(i.Controller, DtoMapper.LeaderBoardSearchUserToDto(i.Post.Body!))),
            handler: new Handler.GetUserLeaderBoardRankSearchUser(),
            outputHandler: new ViewExistOrFoundInfoHandler<List<ViewLeaderBoardUser>>(),
            input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostLeaderBoardSearchUser>>(this.ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}