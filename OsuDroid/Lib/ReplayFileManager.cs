using System.Globalization;
using NPoco;
using OsuDroid.Database.TableFn;
using OsuDroid.Lib.OdrZip;
using OsuDroid.Utils;
using BblScore = OsuDroidLib.Database.Entities.BblScore;
using Path = System.IO.Path;
using Npgsql;

namespace OsuDroid.Lib; 

public static class ReplayFileManager {
    public static ResultErr<string> CreateZipAndOdrFiles(
        SavePoco db, string mapHash, long replayId, long userId, IFormFile odrApiStream) {

        ResultErr<string> odrFileResult = CreateOdrFile(db, mapHash, replayId, userId, odrApiStream.OpenReadStream());

        if (odrFileResult == EResult.Err) return odrFileResult;

        
        ResultErr<string> zipFileResult = CreateOdrZipFile(db, replayId);
        
        if (zipFileResult == EResult.Err) return zipFileResult;
        
        
        return ResultErr<string>.Ok();
    }
    
    public static ResultErr<string> DeleteZipAndOdrFiles(long replayId) {
        var zipPath = "";
        try {
            zipPath = Path.Combine(Env.ReplayZipPath, $"{replayId}.zip");
        }
        catch (Exception e) {
            return ResultErr<string>.Err(e.ToString());
        }
        
        var odrPath = $"{Env.ReplayPath}/{replayId}.odr";

        if (File.Exists(zipPath)) {
            try {
                File.Delete(zipPath);
            }
            catch (Exception) {
                // ignored
            }
        }


        if (File.Exists(odrPath)) {
            try {
                File.Delete(odrPath);
            }
            catch (Exception) {
                // ignored
            }
        }
                

        return ResultErr<string>.Ok();
    }

    public static ResultErr<string> DeleteAllByUserId(SavePoco db, long userId) {
        var sql = new Sql(@$"
SELECT id 
FROM bbl_score
WHERE uid = {userId}
");
        var result = db.Fetch<Entities.BblScore>(sql);
        if (result == EResult.Err)
            return result;
        foreach (var bblScore in result.Ok()) {
            var resultErr = DeleteZipAndOdrFiles(bblScore.Id);
            if (resultErr == EResult.Err)
                return resultErr;
        }

        return ResultErr<string>.Ok();
    }

    public static Result<Option<FileStream>, string> GetReplayOdrByReplayId(SavePoco db, string replayId) {
        long replayIdLong = 0;
        if (long.TryParse(replayId, out replayIdLong) == false) {
            return Result<Option<FileStream>, string>.Err(StackTrace.WithMessage("replayId Is Not A Number"));
        }

        var checkResult = CheckIfScoreExist(db, replayIdLong);
        if (checkResult == EResult.Err)
            return Result<Option<FileStream>, string>.Err(checkResult.Err());
        if (checkResult.Ok() == false)
            return Result<Option<FileStream>, string>.Ok(Option<FileStream>.Empty); 
        
        
        var filePath = $"{Env.ReplayPath}/{replayId}.odr";

        if (System.IO.File.Exists(filePath) == false) {
            return Result<Option<FileStream>, string>.Ok(Option<FileStream>.Empty);
        }

        return Result<Option<FileStream>, string>.Ok(Option<FileStream>.With(System.IO.File.OpenRead(filePath)));
    }

    public static Result<Option<FileStream>, string> GetReplayOdrAsZipByReplayId(SavePoco db, string replayId) {
        long replayIdLong = 0;
        if (long.TryParse(replayId, out replayIdLong) == false) {
            return Result<Option<FileStream>, string>.Err(StackTrace.WithMessage("replayId Is Not A Number"));
        }

        var checkResult = CheckIfScoreExist(db, replayIdLong);
        if (checkResult == EResult.Err)
            return Result<Option<FileStream>, string>.Err(checkResult.Err());

        if (checkResult.Ok())
            return Result<Option<FileStream>, string>.Ok(Option<FileStream>.Empty);
        
        return OsuDroid.Lib.OdrZip.OdrZip
            .Factory(db, replayIdLong)
            .AndThen<Option<FileStream>>(x => {
                if (x.IsSet())
                    return Result<Option<FileStream>, string>.Ok(Option<FileStream>.With(x.Unwrap().stream));
                return Result<Option<FileStream>, string>.Ok(Option<FileStream>.Empty);
            });
    }

    public static ResultErr<string> DeleteAllReplayFilesWithNoScoreInTable() {
        var odr = GetAllIdFromOdrFiles();
        foreach (var s in odr.notValid) {
            try {
                File.Delete(Path.Combine(Env.ReplayPath, s));
            }
            catch (Exception e) {
                WriteLine(e);
            }
        }

        var zip = GetAllIdFromOdrZipFiles();
        foreach (var s in odr.notValid) {
            try {
                File.Delete(Path.Combine(Env.ReplayZipPath, s));
            }
            catch (Exception e) {
                WriteLine(e);
            }
        }

        Result<(long[] existInDb, long[] notExistInDb), string> res = 
            CheckScoreIdsExist(MergeOdrAndZipIds(odr.valid, zip.valid));

        if (res == EResult.Err)
            return res;

        foreach (var id in res.Ok().notExistInDb) {
            DeleteZipAndOdrFiles(id);
            WriteLine($"DELETE Replay Odr Or Zip id: {id}");
        }

        return ResultErr<string>.Ok();
    }

    private static Result<bool, string> CheckIfScoreExist(SavePoco db, long replayId) {
        var sql = new Sql(@$"
SELECT id 
FROM bbl_score 
WHERE id = @0
LIMIT 1
", replayId);

        var result = db.SingleOrDefault<OsuDroidLib.Database.Entities.BblScore>(sql);
        if (result == EResult.Err)
            return Result<bool, string>.Err(result.Err());

        var bblScore = result.OkOrDefault();

        if (bblScore is null)
            return Result<bool, string>.Ok(false);
        return Result<bool, string>.Ok(true);
    }

    private static ResultErr<string> CreateOdrFile(
        SavePoco db, string mapHash, long replayId, long userId, Stream odrApiStream) {
        
        var resultMap = SqlFunc.GetBblScoreByIdAndUserId(db, replayId, userId)
            .Map(x => Option<Entities.BblScore>.NullSplit(x));

        if (resultMap == EResult.Err)
            return ResultErr<string>.Err(resultMap.Err());

        var optionMap = resultMap.Ok();
        
        if (optionMap.IsSet() == false)
            return ResultErr<string>.Err(StackTrace.WithMessage("Map Not Found"));

        var map = optionMap.Unwrap();
        
        var resultOldesMap = SqlFunc
            .GetBblScoreOldesByUserIdAndHash(db, userId, mapHash)
            .Map(x => Option<BblScore>.NullSplit(x));

        if (resultOldesMap == EResult.Err)
            return ResultErr<string>.Err(resultOldesMap.Err());

        var optionOldesMap = resultOldesMap.Ok();
        
        if (optionOldesMap.IsSet() == false)
            return ResultErr<string>.Err(StackTrace.WithMessage("Map Not Found"));

        var oldesMap = optionOldesMap.Unwrap();
        
        if (oldesMap.Id != map.Id)
            return ResultErr<string>.Err(StackTrace.WithMessage("Id Miss Match"));

        if (File.Exists($"{Env.ReplayPath}/{oldesMap.Id}.odr"))
            return ResultErr<string>.Err(StackTrace.WithMessage("Not Allowed; File Exist With The Same Name"));

        using var stream = odrApiStream;
        using var file = File.Create($"{Env.ReplayPath}/{oldesMap.Id}.odr");

        file.Position = 0;
        CopyStream.Move(stream, file);
        file.Flush();
        file.Close();
        stream.Close();
        
        return ResultErr<string>.Ok();
    }

    private static (long[] valid, string[] notValidZip) GetAllIdFromOdrZipFiles() {
        var files = Directory.GetFiles(Env.ReplayZipPath);
        var ids = new List<long>(files.Length);
        var notValid = new List<string>();
        
        foreach (var file in files) {
            var posi = file.LastIndexOf(".zip", StringComparison.Ordinal);
            if (posi == -1) {
                notValid.Add(file);
                continue;
            }
            
            var name = file.Remove(posi);
            if (long.TryParse(name, out var id) == false) {
                notValid.Add(file);
                continue;
            }
            
            ids.Add(id);
        }

        return (ids.ToArray(), notValid.ToArray());
    }
    
    private static (long[] valid, string[] notValid) GetAllIdFromOdrFiles() {
        var files = Directory.GetFiles(Env.ReplayPath);
        var ids = new List<long>(files.Length);
        var notValid = new List<string>();
        
        foreach (var file in files) {
            var posi = file.LastIndexOf(".odr", StringComparison.Ordinal);
            if (posi == -1) {
                notValid.Add(file);
                continue;
            }
            
            var name = file.Remove(posi);
            if (long.TryParse(name, out var id) == false) {
                notValid.Add(file);
                continue;
            }
            
            ids.Add(id);
        }

        return (ids.ToArray(), notValid.ToArray());
    }

    private static HashSet<long> MergeOdrAndZipIds(long[] odrIds, long[] zipIds) {
        var hash = new HashSet<long>(odrIds.Length + zipIds.Length);
        
        foreach (var odrId in odrIds) {
            if (hash.Contains(odrId))
                continue;
            hash.Add(odrId);
        }
        
        foreach (var zipId in zipIds) {
            if (hash.Contains(zipId))
                continue;
            hash.Add(zipId);
        }
        
        return hash;
    }

    private static ResultErr<string> CreateOdrZipFile(SavePoco db, long replayId) {
        var result = OsuDroid.Lib.OdrZip.OdrZip.Factory(db, replayId);
        if (result == EResult.Err)
            return result;
        if (result.Ok().IsSet() == false)
            return ResultErr<string>.Err(StackTrace.WithMessage("No FileStream"));
        
        result.Ok().Unwrap().stream.Close();
        return ResultErr<string>.Ok();
    }

    private static Result<(long[] existInDb, long[] notExistInDb), string> CheckScoreIdsExist(HashSet<long> ids) {
        (long[] existInDb, long[] notExistInDb) res = default;
        try {
            {
                var inList = SqlIn.Builder(ids.Select(x => x.ToString()).ToList());
                using var db = DbBuilder.BuildNpgsqlConnection();
                using var comm = db.CreateCommand();
                comm.CommandText = @$"
SELECT id 
FROM bbl_score
WHERE id in {inList};
";
                using var reader = comm.ExecuteReader();
                var exist = new List<long>(ids.Count);
                var notExist = new List<long>(128);
        
                while (reader.Read()) {
                    var id = reader.GetInt64(0);
                    if (ids.Contains(id) == false) {
                        continue;
                    }
                    exist.Add(id);
                }
                reader.Close();

                foreach (var l in ids) {
                    if (exist.Contains(l))
                        continue;
                    notExist.Add(l);
                }

                res = (exist.ToArray(), notExist.ToArray());
            }

            GC.Collect();
            return Result<(long[] existInDb, long[] notExistInDb), string>.Ok(res);
        }
        catch (Exception e) {
            return Result<(long[] existInDb, long[] notExistInDb), string>.Err(e.ToString());
        }
    }
}
























