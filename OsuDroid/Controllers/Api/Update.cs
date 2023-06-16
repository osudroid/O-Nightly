using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.View;

namespace OsuDroid.Controllers.Api;

public class Update : ControllerExtensions {
    [HttpGet("/api/update")]
    [PrivilegeRoute(route: "/api/update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewApiUpdateInfo))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpdateInfoAsync([FromQuery(Name = "lang")] string lang = "en") {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var dirNameNumber = Directory.GetDirectories(Env.UpdatePath).Select(long.Parse).MaxBy(x => x);
            if (dirNameNumber == 0) return GetInternalServerError();

            var langFiles = Directory.GetFiles($"{Env.UpdatePath}/{dirNameNumber}/changelog");
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

            if (wantFile is null && defaultFile is null) return GetInternalServerError();

            wantFile ??= defaultFile;

            return Ok(new ViewApiUpdateInfo {
                Changelog = await System.IO.File.ReadAllTextAsync($"{Env.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"),
                VersionCode = dirNameNumber,
                Link = $"https://{Env.Domain}/api2/apk/version/{dirNameNumber}.apk"
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