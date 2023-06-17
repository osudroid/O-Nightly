using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.Class;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;

namespace OsuDroid.Controllers.Api2;

public class Api2Avatar : ControllerExtensions {
    [HttpGet("/api2/avatar/{size:long}/{id:long}")]
    [PrivilegeRoute(route: "/api2/avatar/{size:long}/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvatar([FromRoute(Name = "size")] int size, [FromRoute(Name = "id")] long id) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            
            var resultUserAvatar = await log.AddResultAndTransformAsync(
                await UserAvatarHandler.GetByUserIdAsync(db, id, Setting.UserAvatar_SizeLow!.Value >= size));
            
            if (resultUserAvatar == EResult.Err)
                return GetInternalServerError();
            if (resultUserAvatar.Ok().IsNotSet())
                return NotFound();

            var userAvatar = resultUserAvatar.Ok().Unwrap();
            
            var mem = new MemoryStream(userAvatar.Bytes!);
            return File(mem, $"image/{userAvatar.TypeExt}");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api2/avatar/hash/{hash:alpha}")]
    [PrivilegeRoute(route: "/api2/avatar/hash/{hash:alpha}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAvatarHashes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AvatarByHash([FromRoute(Name = "hash")] string? hash) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(
                await UserAvatarHandler.GetByHashAsync(db, hash??""));
            
            if (result == EResult.Err)
                return GetInternalServerError();
            
            if (result.Ok().IsNotSet())
                return NotFound();

            var avatar = result.Ok().Unwrap();
            
            return File(avatar.Bytes!, $"image/{avatar.TypeExt}");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpPost("/api2/avatar/hash")]
    [PrivilegeRoute(route: "/api2/avatar/hash")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAvatarHashes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AvatarHashesByUserIds([FromBody] PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest();
            
            var size = (Setting.UserAvatar_SizeLow!.Value >= prop.Body!.Size) 
                ? Setting.UserAvatar_SizeLow!.Value 
                : Setting.UserAvatar_SizeHigh!.Value;
            var resp = await log.AddResultAndTransformAsync(await db.SafeQueryAsync<UserAvatar>(@$"
SELECT UserId, Hash
FROM UserAvatar
WHERE PixelSize = {size}
AND UserId in @UserIds
", new { UserIds = prop.Body.UserIds }));

            if (resp == EResult.Err)
                return GetInternalServerError();
            
            return Ok(new ViewAvatarHashes {
                List = resp.Ok().Select(x => new ViewAvatarHash { Hash = x.Hash, UserId = x.UserId }).ToList()
            });
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}
