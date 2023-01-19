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


    public static Response<(FileStream stream, string name)> Factory(SavePoco db, long odrNumber) {
        var bblScore = db.SingleOrDefault<BblScore>("SELECT * FROM bbl_score WHERE id = " + odrNumber).OkOrDefault();

        if (bblScore is null)
            return Response<(FileStream stream, string name)>.Err;

        var filename = odrNumber + ".zip";
        FileStream stream;
        if (File.Exists(Env.ReplayZipPath)) {
            stream = File.OpenRead(Path.Combine(Env.ReplayZipPath, filename));
        }
        else {
            var bblUser = db.SingleOrDefault<BblUser>(
                $"SELECT username FROM bbl_user WHERE id = {bblScore.Uid}").OkOrDefault();

            if (bblUser is null || bblUser.Username is null)
                return Response<(FileStream stream, string name)>.Err;

            var odrPath = Path.Join(Env.ReplayPath, odrNumber + ".odr");
            if (Path.Exists(odrPath) == false)
                return Response<(FileStream stream, string name)>.Err;

            stream = CreateOdrZip(File.OpenRead(odrPath), OdrEntry.Factory(bblScore, bblUser.Username), filename);
        }

        stream.Position = 0;
        return Response<(FileStream stream, string name)>.Ok((stream, $"{bblScore.Filename}_{odrNumber}.zip"));
    }
}