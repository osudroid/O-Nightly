using System.Data;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroidLib.Class;

namespace OsuDroid.Controllers.Api2;

public class Api2JarHasher : ControllerExtensions {
    [HttpGet("/api2/jar/version/{version}.jar")]
    [PrivilegeRoute(route: "/api2/jar/version/{version}.jar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Jar(
        [FromRoute(Name = "version")] string version, [FromQuery(Name = "q")] string keyToken) {
        
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            if (Setting.RequestHash_Keyword!.Value != keyToken) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return BadRequest("Bad RequestHash_Keyword");
            }
            
            var path = $"{Setting.JarPath}/{version}.jar";
            if (System.IO.File.Exists(path) == false) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                await log.AddLogDebugAsync($"File Not Found In {path}");
                return BadRequest("Not Found");
            }


            try {
                await log.AddLogOkAsync("Send File");
                return File(System.IO.File.OpenRead(path), "application/apk");
            }
            catch (Exception e) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }
                await log.AddResultAndTransformAsync(Result<string,string>.Err(e.ToString()));
                return InternalServerError();
            }
        }
        catch (Exception e) {
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }
            await log.AddResultAndTransformAsync(Result<string,string>.Err(e.ToString()));
            return InternalServerError();
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}