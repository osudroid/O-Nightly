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

public class Api2Login : ControllerExtensions {
    [HttpPost("/api2/token-create")]
    [PrivilegeRoute("/api2/token-create")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewCreateApi2TokenResult>))]
    public async Task<IActionResult> CreateApi2TokenAsync(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostCreateApi2Token> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new CreateApi2TokenValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateApi2Token>>,
                ControllerPostWrapper<CreateApi2TokenDto>>(i
                => new ControllerPostWrapper<CreateApi2TokenDto>(i.Controller,
                    DtoMapper.CreateApi2TokenToDto(i.Post.Body!))),
            new CreateApi2TokenHandler(),
            new ViewExistOrFoundInfoHandler<ViewCreateApi2TokenResult>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateApi2Token>>(ControllerHandlerBuild(),
                prop)
        );

        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/token-refresh")]
    [PrivilegeRoute("/api2/token-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshApi2TokenAsync(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new SimpleTokenValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                ControllerPostWrapper<SimpleTokenDto>>(i
                => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
            new RefreshApi2TokenHandler(),
            new WorkHandler(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/token-remove")]
    [PrivilegeRoute("/api2/token-remove")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveApi2Token([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new SimpleTokenValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                ControllerPostWrapper<SimpleTokenDto>>(i
                => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
            new RemoveApi2TokenHandler(),
            new WorkHandler(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/token-user-id")]
    [PrivilegeRoute("/api2/token-user-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<long>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTokenUserId([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new SimpleTokenValidation(),
            new TransformAction<
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                ControllerPostWrapper<SimpleTokenDto>>(i
                => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
            new GetTokenUserIdHandler(),
            new ViewExistOrFoundInfoHandler<long>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}