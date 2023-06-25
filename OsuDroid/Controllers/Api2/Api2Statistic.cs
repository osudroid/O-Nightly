using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Class;
using OsuDroidLib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

public class Api2Statistic : ControllerExtensions {
    [HttpGet("/api2/statistic/active-user")]
    [PrivilegeRoute(route: "/api2/statistic/active-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>))]
    public async Task<IActionResult> GetActiveUser() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(await ModelStatistic.ActiveUserAsync(db));
            if (result == EResult.Err)
                return Ok(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>.NotExist());

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
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }

    [HttpGet("/api2/statistic/all-patreon")]
    [PrivilegeRoute(route: "/api2/statistic/all-patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>))]
    public async Task<IActionResult> GetAllPatreon() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var result = await log.AddResultAndTransformAsync(await ModelStatistic.ActiveUserAsync(db));
            if (result == EResult.Err)
                return Ok(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>.NotExist());

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
            return await RollbackAndGetInternalServerErrorAsync(dbT);
        }
        finally {
            await dbT.CommitAsync();
        }
    }
}
