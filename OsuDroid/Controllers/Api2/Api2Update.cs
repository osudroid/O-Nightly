using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;

namespace OsuDroid.Controllers.Api2;

public class Api2Update : ControllerExtensions {
    [HttpGet("/api2/update/{lang}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiUpdateInfoV2))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUpdateInfoV2([FromRoute(Name = "lang")] string lang = "en") {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
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

        return Ok(new ApiUpdateInfoV2 {
            Changelog = System.IO.File.ReadAllText($"{Env.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"),
            VersionCode = dirNameNumber,
            Link = $"https://{Env.Domain}/api2/apk/version/{dirNameNumber}.apk"
        });
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ApiUpdateInfoV2 {
        public long VersionCode { get; set; }
        public string? Link { get; set; }
        public string? Changelog { get; set; }
    }
}