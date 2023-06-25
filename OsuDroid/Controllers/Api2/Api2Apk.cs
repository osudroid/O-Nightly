using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;

namespace OsuDroid.Controllers.Api2;

public class Api2Apk : ControllerExtensions {
    [HttpGet("/api2/apk/version/{dirNameNumber:long}.apk")]
    [PrivilegeRoute(route: "/api2/apk/version/{dirNameNumber:long}.apk")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpdateInfo([FromRoute(Name = "dirNameNumber")] long version) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var path = $"{Setting.UpdatePath}/{version}/android.apk";

            if (System.IO.File.Exists(path) == false)
                return await RollbackAndGetBadRequestAsync(dbT, "Version Number not exist");

            await using var fileStream = System.IO.File.OpenRead(path);
            return File(fileStream, "application/apk");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}
