using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib.OdrZip;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Odr : ControllerExtensions {
    [HttpGet("/api2/odr/{replayId}.odr")]
    public ActionResult GetOdrFile([FromRoute(Name = "replayId")] string replayId) {
        var filePath = $"{Env.ReplayPath}/{replayId}.odr";

        if (System.IO.File.Exists(filePath) == false) return BadRequest("File Not Exist");

        return File(System.IO.File.OpenRead(filePath), "Application/octet-stream");
    }

    [HttpGet("/api2/odr/{replayId:long}.zip")]
    public ActionResult GetOdrZipFile([FromRoute(Name = "replayId")] long replayId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var res = OdrZip.Factory(db, replayId);

        if (res == EResponse.Err) return BadRequest();

        var (stream, name) = res.Ok();

        return File(stream, "Application/octet-stream");
    }

    [HttpGet("/api2/odr/fullname/{replayId:long}/{fullname}.zip")]
    public ActionResult GetOdrZipFileWithName([FromRoute(Name = "replayId")] long replayId,
        [FromRoute(Name = "fullname")] string fullname) {
        return GetOdrZipFile(replayId);
    }

    [HttpGet("/api2/odr/redirect/{replayId:long}.zip")]
    public ActionResult GetOdrZipFileRedHandler([FromRoute(Name = "replayId")] long replayId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var bblScore = db.SingleOrDefault<BblScore>($"SELECT filename, date, uid FROM bbl_score WHERE id = {replayId}")
            .OkOrDefault();

        if (bblScore is null)
            return BadRequest();

        var fullname = $"{bblScore.Filename!.Replace(".osu", "")} {bblScore.Uid} {bblScore.Date.Ticks}";
        return RedirectPermanent($"/api/upload/fullname/{replayId}/{fullname}.zip");
    }
}