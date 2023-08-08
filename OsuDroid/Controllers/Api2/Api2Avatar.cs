using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.HttpGet;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;

namespace OsuDroid.Controllers.Api2;

public class Api2Avatar : ControllerExtensions {
    [HttpGet("/api2/avatar/{size:long}/{id:long}")]
    [PrivilegeRoute("/api2/avatar/{size:long}/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvatar([FromRoute(Name = "size")] int size, [FromRoute(Name = "id")] long id) {
        var prop = new GetAvatar { Size = size, Id = id };

        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new GetAvatarValidation(),
            new TransformParse<ControllerGetWrapper<GetAvatar>>(),
            new GetAvatarHandler(),
            new ViewImage<IActionResult> {
                Converter = i => File(i.Bytes, $"image/{i.Ext}")
            },
            new ControllerGetWrapper<GetAvatar>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpGet("/api2/avatar/hash/{hash:alpha}")]
    [PrivilegeRoute("/api2/avatar/hash/{hash:alpha}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAvatarHashes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AvatarByHash([FromRoute(Name = "hash")] string? hash) {
        var prop = new GetAvatarWithHash { Hash = hash ?? "" };

        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new GetAvatarByHashValidation(),
            new TransformParse<ControllerGetWrapper<GetAvatarWithHash>>(),
            new GetAvatarByHashHandler(),
            new ViewImage<IActionResult> {
                Converter = i => File(i.Bytes, $"image/{i.Ext}")
            },
            new ControllerGetWrapper<GetAvatarWithHash>(ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/avatar/hash")]
    [PrivilegeRoute("/api2/avatar/hash")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewAvatarHashes>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AvatarHashesByUserIds(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds> prop) {
        var transaction = await Service.AttachmentServiceApi(
            new NpgsqlCreates(),
            new LogCreates(),
            new AvatarHashesByUserIdsValidation(),
            new TransformAction<ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>>,
                ControllerPostWrapper<AvatarHashesByUserIdsDto>>(
                i => new ControllerPostWrapper<AvatarHashesByUserIdsDto>(
                    i.Controller, DtoMapper.AvatarHashesByUserIdsToDto(i.Post.Body!))),
            new GetAvatarHashesByUserIdsHandler(),
            new ViewExistOrFoundInfoHandler<ViewAvatarHashes>(),
            new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>>(
                ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}