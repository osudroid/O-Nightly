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
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        var isComplete = false;
        
        try {
            var dirNameNumber = Directory.GetDirectories(Setting.UpdatePath!).Select(long.Parse).MaxBy(x => x);
            if (dirNameNumber == 0) return await RollbackAndGetInternalServerErrorAsync(dbT);

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

            if (wantFile is null && defaultFile is null)
                return await RollbackAndGetInternalServerErrorAsync(dbT);

            wantFile ??= defaultFile;

            return Ok(new ViewApiUpdateInfoV2 {
                Changelog = await System.IO.File.ReadAllTextAsync(
                    $"{Setting.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"),
                VersionCode = dirNameNumber,
                Link = $"https://{Setting.Domain_Name!.Value}/api2/apk/version/{dirNameNumber}.apk"
            });
        }
        catch (Exception e) {
            isComplete = true;
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            if (!isComplete) {
                await dbT.CommitAsync();
            }
        }
    }
}