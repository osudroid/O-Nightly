using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;

namespace OsuDroid.Controllers.Api2;

public class Api2Apk : ControllerExtensions {
    [HttpGet("/api2/apk/version/{dirNameNumber:long}.apk")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUpdateInfo([FromRoute(Name = "dirNameNumber")] long version) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        try {
            using var fileStream = System.IO.File.OpenRead($"{Env.UpdatePath}/{version}/android.apk");
            return File(fileStream, "application/apk");
        }
#if DEBUG
        catch (Exception e) {
            throw;
        }
#else
        catch (Exception) {
            return BadRequest();
        }
#endif
    }
}