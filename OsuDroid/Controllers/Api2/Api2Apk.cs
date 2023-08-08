using System.Data;
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
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            var path = $"{Setting.UpdatePath}/{version}/android.apk";

            if (System.IO.File.Exists(path) == false) {
                return BadRequest("Version Number not exist");
            }
            
            await using var fileStream = System.IO.File.OpenRead(path);
            return File(fileStream, "application/apk");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync(e.ToString());
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }
            return InternalServerError();
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}