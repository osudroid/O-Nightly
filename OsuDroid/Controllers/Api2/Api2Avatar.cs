using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.View;
using OsuDroid.OutputHandler;
using OsuDroid.Validation;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;

namespace OsuDroid.Controllers.Api2;

public class Api2Avatar : ControllerExtensions {
    [HttpGet("/api2/avatar/{size:long}/{id:long}")]
    [PrivilegeRoute(route: "/api2/avatar/{size:long}/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvatar([FromRoute(Name = "size")] int size, [FromRoute(Name = "id")] long id) {
        var prop = new HttpGet.GetAvatar { Size = size, Id = id };
        
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new GetAvatarValidation(),
            transformHandler: new TransformParse<ControllerGetWrapper<HttpGet.GetAvatar>>(),
            handler: new Handler.GetAvatarHandler(),
            outputHandler: new ViewImage<IActionResult>() {
                Converter = (i) => File(i.Bytes, $"image/{i.Ext}")
            },
            input: new ControllerGetWrapper<HttpGet.GetAvatar>(this.ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpGet("/api2/avatar/hash/{hash:alpha}")]
    [PrivilegeRoute(route: "/api2/avatar/hash/{hash:alpha}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAvatarHashes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AvatarByHash([FromRoute(Name = "hash")] string? hash) {
        var prop = new HttpGet.GetAvatarWithHash { Hash = hash??"" };
        
        Transaction<IActionResult> transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new GetAvatarByHashValidation(),
            transformHandler: new TransformParse<ControllerGetWrapper<HttpGet.GetAvatarWithHash>>(),
            handler: new Handler.GetAvatarByHashHandler(),
            outputHandler: new ViewImage<IActionResult>() {
                Converter = (i) => File(i.Bytes, $"image/{i.Ext}")
            },
            input: new ControllerGetWrapper<HttpGet.GetAvatarWithHash>(this.ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }

    [HttpPost("/api2/avatar/hash")]
    [PrivilegeRoute(route: "/api2/avatar/hash")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewAvatarHashes>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AvatarHashesByUserIds(
        [FromBody] PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds> prop) {
        
        var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi(
            dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
            loggerCreates: new Class.LogCreates(),
            validationHandler: new AvatarHashesByUserIdsValidation(),
            transformHandler: new TransformAction<ControllerPostWrapper<
                PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>>, 
                ControllerPostWrapper<AvatarHashesByUserIdsDto>>(
                (i) => new ControllerPostWrapper<AvatarHashesByUserIdsDto>(                 
                    i.Controller, DtoMapper.AvatarHashesByUserIdsToDto(i.Post.Body!))),
            handler: new Handler.GetAvatarHashesByUserIdsHandler(),
            outputHandler: new ViewExistOrFoundInfoHandler<ViewAvatarHashes>(),
            input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>>(this.ControllerHandlerBuild(), prop)
        );
        return TransactionToIResult(transaction);
    }
}