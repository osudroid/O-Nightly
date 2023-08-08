using System.Data;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.View;

namespace OsuDroid.Controllers.Api2;

public class Api2Update : ControllerExtensions {
    [HttpGet("/api2/update/{lang}")]
    [PrivilegeRoute(route: "/api2/update/{lang}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewApiUpdateInfoV2))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpdateInfoV2Async([FromRoute(Name = "lang")] string lang = "en") {
        await using var dbN = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = OsuDroidLib.Log.GetLog(db);
        var isComplete = false;
        
        try {
            var dirNameNumber = Directory.GetDirectories(Setting.UpdatePath!).Select(long.Parse).MaxBy(x => x);
            if (dirNameNumber == 0) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return InternalServerError();
            }

            var langFiles = Directory.GetFiles($"{Setting.UpdatePath}/{dirNameNumber}/changelog");
            string? defaultFile = null;
            string? wantFile = null;
            foreach (var langFile in langFiles) {
                if (langFile == "en")
                    defaultFile = "en";
                if (langFile != lang)
                    continue;
                wantFile = langFile;
                break;
            }

            if (wantFile is null && defaultFile is null) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return InternalServerError();
            }

            wantFile ??= defaultFile;

            return Ok(new ViewApiUpdateInfoV2 {
                Changelog = await System.IO.File.ReadAllTextAsync(
                    $"{Setting.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"),
                VersionCode = dirNameNumber,
                Link = $"https://{Setting.Domain_Name!.Value}/api2/apk/version/{dirNameNumber}.apk"
            });
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

            await log.FlushToDbAsync();
        }
    }
}