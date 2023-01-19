using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OsuDroid.Extensions;

namespace OsuDroid.Controllers.Api;

public class Update : ControllerExtensions {
    [HttpGet("/api/update.php")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiUpdateInfo))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUpdateInfo([FromQuery(Name = "lang")] string lang = "en") {
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

        return Ok(new ApiUpdateInfo {
            Changelog = System.IO.File.ReadAllText($"{Env.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"),
            VersionCode = dirNameNumber,
            Link = $"https://{Env.Domain}/api2/apk/version/{dirNameNumber}.apk"
        });
    }

    public class ApiUpdateInfo {
        [JsonProperty]
        [JsonPropertyName("version_code")]
        public long VersionCode { get; set; }

        [JsonProperty]
        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonProperty]
        [JsonPropertyName("changelog")]
        public string? Changelog { get; set; }
    }
}