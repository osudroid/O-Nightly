using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;

namespace OsuDroid.Controllers.Api2;

public class Api2JarHasher : ControllerExtensions {
    [HttpGet("/api2/jar/version/{version}.jar")]
    [PrivilegeRoute(route: "/api2/jar/version/{version}.jar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Jar([FromRoute(Name = "version")] string version, [FromQuery(Name = "q")] string keyToken) {
        await using var start = await GetStartAsync();
        var (db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        if (Env.Keyword != keyToken)
            return BadRequest();
        
        var path = $"{Env.JarPath}/{version}.jar";
        if (System.IO.File.Exists(path) == false) {
            await log.AddLogDebugAsync($"File Not Found In {path}");
            return BadRequest("Not Found");
        }
            
        
        try {
            var fileStream = System.IO.File.OpenRead(path);
            await log.AddLogOkAsync("Send File");
            return File(fileStream, "application/apk");
        }
        catch (Exception) {
            return GetInternalServerError();
        }
    }
}
