using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Lib.OdrZip;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

public class Api2Odr : ControllerExtensions {
    [HttpGet("/api2/odr/{replayId}.odr")]
    [PrivilegeRoute(route: "/api2/odr/{replayId}.odr")]
    public async Task<IActionResult> GetOdrFile([FromRoute(Name = "replayId")] string replayId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var filePath = $"{Setting.ReplayPath}/{replayId}.odr";

            if (System.IO.File.Exists(filePath) == false)
                return await RollbackAndGetBadRequestAsync(dbT, "File Not Exist");

            return File(System.IO.File.OpenRead(filePath), "Application/octet-stream");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api2/odr/{replayId:long}.zip")]
    [PrivilegeRoute(route: "/api2/odr/{replayId:long}.zip")]
    public async Task<IActionResult> GetOdrZipFileAsync([FromRoute(Name = "replayId")] long replayId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var res = (await log.AddResultAndTransformAsync(await OdrZip.FactoryAsync(db, replayId)))
                         .OkOr(Option<(FileStream stream, string name)>.Empty);

            if (res.IsSet() == false)
                return await RollbackAndGetBadRequestAsync(dbT);

            var (stream, name) = res.Unwrap();

            return File(stream, "Application/octet-stream");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api2/odr/fullname/{replayId:long}/{fullname}.zip")]
    [PrivilegeRoute(route: "/api2/odr/fullname/{replayId:long}/{fullname}.zip")]
    public async Task<IActionResult> GetOdrZipFileWithName([FromRoute(Name = "replayId")] long replayId,
        [FromRoute(Name = "fullname")] string fullname) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            return await GetOdrZipFileAsync(replayId);
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api2/odr/redirect/{replayId:long}.zip")]
    [PrivilegeRoute(route: "/api2/odr/redirect/{replayId:long}.zip")]
    public async Task<IActionResult> GetOdrZipFileRedHandler([FromRoute(Name = "replayId")] long replayId) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var bblScoreOption = (await log.AddResultAndTransformAsync(await QueryPlayScore
                .GetByIdAsync(db, replayId)))
                .OkOr(Option<PlayScore>.Empty);

            if (bblScoreOption.IsNotSet())
                return await RollbackAndGetBadRequestAsync(dbT);

            var bblScore = bblScoreOption.Unwrap();
            var fullname = $"{bblScore.Filename!.Replace(".osu", "")} {bblScore.UserId} {bblScore.Date.Ticks}";
            return RedirectPermanent($"/api/upload/fullname/{replayId}/{fullname}.zip");
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}
