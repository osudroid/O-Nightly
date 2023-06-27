using System.IO.Compression;
using Newtonsoft.Json;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroid.Lib.OdrZip;

public class OdrZip {
    public required FileStream Stream { get; init; }

    private static FileStream CreateOdrZip(FileStream odrStream, OdrEntry entry, string fileName) {
        var file = File.Create(Path.Combine(Setting.ReplayZipPath!, fileName), 4096 * 2);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create, true);

        {
            var odrEntry = archive.CreateEntry(entry.Replay!.Replayfile!, CompressionLevel.SmallestSize);
            using (var odrEntryStream = odrEntry.Open()) {
                CopyStream.Move(odrStream, odrEntryStream);
            }
        }

        {
            var entryJson = archive.CreateEntry("entry.json", CompressionLevel.SmallestSize);
            using (var odrEntryStream = entryJson.Open()) {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry, Formatting.None));
                odrEntryStream.Write(new Span<byte>(bytes));
            }
        }

        file.Flush();
        return file;
    }


    public static async Task<Result<Option<(FileStream stream, string name)>, string>> FactoryAsync(NpgsqlConnection db,
        long odrNumber) {
        var resultBblScore = await QueryPlayScore.GetByIdAsync(db, odrNumber);

        if (resultBblScore == EResult.Err)
            return resultBblScore.ChangeOkType<Option<(FileStream stream, string name)>>();

        var optionBblScore = resultBblScore.Ok();
        if (optionBblScore.IsSet() == false)
            return Result<Option<(FileStream stream, string name)>, string>.Ok(Option<(FileStream stream, string name)>
                .Empty);

        var bblScore = optionBblScore.Unwrap();

        var filename = odrNumber + ".zip";
        FileStream stream;
        if (File.Exists(Setting.ReplayZipPath)) {
            stream = File.OpenRead(Path.Combine(Setting.ReplayZipPath, filename));
        }
        else {
            var resultBblUser = await QueryUserInfo.GetUsernameByUserIdAsync(db, bblScore.PlayScoreId);

            if (resultBblUser == EResult.Err)
                return resultBblScore.ChangeOkType<Option<(FileStream stream, string name)>>();

            var optionBblUser = resultBblUser.Ok();
            if (optionBblUser.IsSet() == false)
                return Result<Option<(FileStream stream, string name)>, string>.Ok(
                    Option<(FileStream stream, string name)>.Empty);

            var odrPath = Path.Join(Setting.ReplayPath, odrNumber + ".odr");
            if (Path.Exists(odrPath) == false)
                return Result<Option<(FileStream stream, string name)>, string>.Err($"Path Error odrPath: {odrPath}");

            stream = CreateOdrZip(File.OpenRead(odrPath), OdrEntry
                .Factory(bblScore, optionBblUser.Unwrap().Username ?? ""), filename);
        }

        stream.Position = 0;
        return Result<Option<(FileStream stream, string name)>, string>
            .Ok(Option<(FileStream stream, string name)>.With((stream, $"{bblScore.Filename}_{odrNumber}.zip")));
    }
}