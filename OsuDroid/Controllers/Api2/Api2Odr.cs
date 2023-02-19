using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.OdrZip;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Odr : ControllerExtensions {
    [HttpGet("/api2/odr/{replayId}.odr")]
    public ActionResult GetOdrFile([FromRoute(Name = "replayId")] string replayId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);

        var result = log.AddResultAndTransform(ReplayFileManager.GetReplayOdrByReplayId(db, replayId));
        if (result == EResult.Err)
            return this.GetInternalServerError();// "Server Error"
        
        
        
        FileStream? fileStream = result.OkOrDefault().OrNull();

        if (fileStream is null)
            return BadRequest("File Not Exist");

        return File(fileStream, "Application/octet-stream");
    }

    [HttpGet("/api2/odr/{replayId:long}.zip")]
    public ActionResult GetOdrZipFile([FromRoute(Name = "replayId")] long replayId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();

        var res = log
            .AddResultAndTransform(Lib.ReplayFileManager.GetReplayOdrAsZipByReplayId(db, replayId.ToString()))
            .OkOr(Option<FileStream>.Empty);
        
        if (res.IsSet() == false) return BadRequest();

        return File(res.Unwrap(), "Application/octet-stream");
    }

    [HttpGet("/api2/odr/fullname/{replayId:long}/{fullname}.zip")]
    public ActionResult GetOdrZipFileWithName([FromRoute(Name = "replayId")] long replayId,
        [FromRoute(Name = "fullname")] string fullname) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        return GetOdrZipFile(replayId);
    }

    [HttpGet("/api2/odr/redirect/{replayId:long}.zip")]
    public ActionResult GetOdrZipFileRedHandler([FromRoute(Name = "replayId")] long replayId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        var bblScore = log.AddResultAndTransform(db.SingleOrDefault<BblScore>(
                $"SELECT filename, date, uid FROM bbl_score WHERE id = {replayId}"))
            .OkOrDefault();

        if (bblScore is null)
            return BadRequest();

        var fullname = $"{bblScore.Filename!.Replace(".osu", "")} {bblScore.Uid} {bblScore.Date.Ticks}";
        return RedirectPermanent($"/api/upload/fullname/{replayId}/{fullname}.zip");
    }
}