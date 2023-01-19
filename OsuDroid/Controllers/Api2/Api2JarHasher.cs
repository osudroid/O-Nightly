using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;

namespace OsuDroid.Controllers.Api2;

public class Api2JarHasher : ControllerExtensions {
    [HttpGet("/api2/jar/version/{version}.jar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Jar([FromRoute(Name = "version")] string version, [FromQuery(Name = "q")] string keyToken) {
        if (Env.Keyword != keyToken)
            return BadRequest();
        try {
            var fileStream = System.IO.File.OpenRead($"{Env.JarPath}/{version}.jar");
            return File(fileStream, "application/apk");
        }
        catch (Exception) {
            return GetInternalServerError();
        }
    }
}