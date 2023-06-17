using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Class;
using OsuDroid.Model;

namespace OsuDroid.Controllers.Api;

public class Update : ControllerExtensions {
    [HttpGet("/api/update")]
    [PrivilegeRoute(route: "/api/update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewApiUpdateInfo))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpdateInfoAsync([FromQuery(Name = "lang")] string lang = "en") {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(await ModelApiUpdate.GetUpdateInfoAsync(
                this, db, lang));
            
            if (result == EResult.Err) {
                return await RollbackAndGetInternalServerErrorAsync(dbT);
            }

            return result.Ok().Mode switch {
                EModelResult.Ok => Ok(result.Ok().Result.Unwrap()),
                EModelResult.BadRequest => await RollbackAndGetBadRequestAsync(dbT),
                EModelResult.InternalServerError => await RollbackAndGetInternalServerErrorAsync(dbT),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e) {
            await log.AddLogErrorAsync("ERROR", Option<string>.With(e.ToString()));
            await dbT.RollbackAsync();
            return GetInternalServerError();
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}