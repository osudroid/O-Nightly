using System.IO.Compression;
using Newtonsoft.Json;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.OdrZip;

public class OdrZip {
    public required FileStream Stream { get; init; }

    private static FileStream CreateOdrZip(FileStream odrStream, OdrEntry entry, string fileName) {
        var file = File.Create(Path.Combine(Env.ReplayZipPath, fileName), 4096 * 2);
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


    public static Result<Option<(FileStream stream, string name)>, string> Factory(SavePoco db, long odrNumber) {
        var resultBblScore = db
            .SingleOrDefault<BblScore>("SELECT * FROM bbl_score WHERE id = " + odrNumber)
            .Map(x => Option<BblScore>.NullSplit(x));

        if (resultBblScore == EResult.Err)
            return Result<Option<(FileStream stream, string name)>, string>.Err(resultBblScore.Err());
        
        var optionBblScore = resultBblScore.Ok();
        if (optionBblScore.IsSet() == false)
            return Result<Option<(FileStream stream, string name)>, string>.Ok(Option<(FileStream stream, string name)>.Empty);

        var bblScore = optionBblScore.Unwrap(); 
        
        var filename = odrNumber + ".zip";
        FileStream stream;
        if (File.Exists(Env.ReplayZipPath)) {
            stream = File.OpenRead(Path.Combine(Env.ReplayZipPath, filename));
        }
        else {
            var resultBblUser = db.SingleOrDefault<BblUser>(
                $"SELECT username FROM bbl_user WHERE id = {bblScore.Uid}").Map(x => Option<BblUser>.NullSplit(x));
            
            if (resultBblUser == EResult.Err)
                return Result<Option<(FileStream stream, string name)>, string>.Err(resultBblScore.Err());
            
            var optionBblUser = resultBblUser.Ok();
            if (optionBblUser.IsSet() == false)
                return Result<Option<(FileStream stream, string name)>, string>.Ok(Option<(FileStream stream, string name)>.Empty);

            var odrPath = Path.Join(Env.ReplayPath, odrNumber + ".odr");
            if (Path.Exists(odrPath) == false)
                return Result<Option<(FileStream stream, string name)>, string>.Err($"Path Error odrPath: {odrPath}");

            stream = CreateOdrZip(File.OpenRead(odrPath), OdrEntry
                .Factory(bblScore, optionBblUser.Unwrap().Username??""), filename);
        }

        stream.Position = 0;
        return Result<Option<(FileStream stream, string name)>, string>
            .Ok(Option<(FileStream stream, string name)>.With((stream, $"{bblScore.Filename}_{odrNumber}.zip")));
    }
}