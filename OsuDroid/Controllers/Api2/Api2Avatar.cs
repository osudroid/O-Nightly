using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroidLib.Database.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace OsuDroid.Controllers.Api2;

public class Api2Avatar : ControllerExtensions {
    [HttpGet("/api2/avatar/{size:long}/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAvatar([FromRoute(Name = "size")] int size, [FromRoute(Name = "id")] long id) {
        var filePath = $"{Env.AvatarPath}/" + id;

        var bytes = System.IO.File.Exists(filePath) switch {
            true => System.IO.File.ReadAllBytes(filePath),
            false => System.IO.File.ReadAllBytes($"{Env.AvatarPath}/default.jpg")
        };

        var imageMemoryStream = new MemoryStream(bytes);

        var image = Image.Load(imageMemoryStream);
        image.Mutate(x => x.Resize(size, size));

        var imageMemoryRes = new MemoryStream();
        image.SaveAsPng(imageMemoryRes);
        imageMemoryRes.Position = 0;
        image.Dispose();
        var file = File(imageMemoryRes, "image/png");
        return file;
    }

    [HttpGet("/api2/avatar/hash/{size:long}/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AvatarHashes))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AvatarHashByUserId([FromRoute(Name = "size")] int size, [FromRoute(Name = "id")] long id) {
        if (size > 1000 || size < 1 || id < 1) return BadRequest();

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var resp = db.Fetch<BblAvatarHash>(@$"
SELECT user_id, hash 
FROM bbl_avatar_hash
WHERE size = {size}
AND user_id = {id}
");

        return Ok(new AvatarHashes {
            List = (EResponse)resp switch {
                EResponse.Err => new List<AvatarHash>(),
                _ => resp.Ok().Select(x => new AvatarHash { Hash = x.Hash, UserId = x.UserId }).ToList()
            }
        });
    }

    [HttpPost("/api2/avatar/hash")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AvatarHashes))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AvatarHashesByUserIds([FromBody] ApiTypes.Api2GroundNoHeader<AvatarHashesByUserIdsProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var resp = db.Fetch<BblAvatarHash>(@$"
SELECT user_id, hash 
FROM bbl_avatar_hash
WHERE size = {prop.Body!.Size}
AND user_id in ({string.Join(',', prop.Body.UserIds ?? Array.Empty<long>())})
");

        return Ok(new AvatarHashes {
            List = (EResponse)resp switch {
                EResponse.Err => new List<AvatarHash>(),
                _ => resp.Ok().Select(x => new AvatarHash { Hash = x.Hash, UserId = x.UserId }).ToList()
            }
        });
    }

    public class AvatarHashesByUserIdsProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString {
        public int Size { get; set; }
        public long[]? UserIds { get; set; }

        public string ToSingleString() {
            return Merge.ListToString(new object[] {
                Size,
                Merge.ListToString(UserIds ?? Array.Empty<long>())
            });
        }

        public bool ValuesAreGood() {
            return !(
                Size > 1000
                || Size < 1
                || UserIds is null or { Length: > 1000 or 0 });
        }
    }

    public class AvatarHash {
        public long UserId { get; set; }
        public string? Hash { get; set; }
    }

    public class AvatarHashes {
        public List<AvatarHash>? List { get; set; }
    }
}