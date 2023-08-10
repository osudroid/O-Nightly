using Npgsql;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.View;

namespace OsuDroid.Model;

public static class ModelApiUpdate {
    public static async Task<Result<ModelResult<ViewApiUpdateInfo>, string>> GetUpdateInfoAsync(
        ControllerExtensions controller,
        NpgsqlConnection db,
        string lang) {
        var dirNameNumber = Directory.GetDirectories(Setting.UpdatePath!).Select(long.Parse).MaxBy(x => x);
        if (dirNameNumber == 0)
            return Result<ModelResult<ViewApiUpdateInfo>, string>
                .Ok(ModelResult<ViewApiUpdateInfo>.BadRequest());

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
            return Result<ModelResult<ViewApiUpdateInfo>, string>
                .Err(TraceMsg.WithMessage("wantFile is null && defaultFile is null"));

        wantFile ??= defaultFile;

        return Result<ModelResult<ViewApiUpdateInfo>, string>
            .Ok(ModelResult<ViewApiUpdateInfo>.Ok(new ViewApiUpdateInfo {
                        Changelog = await File.ReadAllTextAsync(
                            $"{Setting.UpdatePath}/{dirNameNumber}/changelog/{wantFile}"
                        ),
                        VersionCode = dirNameNumber,
                        Link = $"https://{Setting.Domain_Name!.Value}/api2/apk/version/{dirNameNumber}.apk"
                    }
                )
            );
    }
}