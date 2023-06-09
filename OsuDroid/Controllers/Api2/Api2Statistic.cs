using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroidLib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

public class Api2Statistic : ControllerExtensions {
    [HttpGet("/api2/statistic/active-user")]
    [PrivilegeRoute(route: "/api2/statistic/active-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<QueryUserInfo.StatisticActiveUser>))]
    public async Task<IActionResult> GetActiveUser() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var rep = await log.AddResultAndTransformAsync(await Statistic.ActiveUserAsync(db));
            if (rep == EResult.Err)
                return Ok(ApiTypes.ExistOrFoundInfo<QueryUserInfo.StatisticActiveUser>.NotExist());

            var value = rep.Ok();
            return Ok(ApiTypes.ExistOrFoundInfo<QueryUserInfo.StatisticActiveUser>.Exist(new QueryUserInfo
                .StatisticActiveUser { 
                    RegisterUser = value.Register, 
                    ActiveUserLast1H = value.Last1h, 
                    ActiveUserLast1Day = value.Last1Day
            }));
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

    [HttpGet("/api2/statistic/all-patreon")]
    [PrivilegeRoute(route: "/api2/statistic/all-patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>))]
    public async Task<IActionResult> GetAllPatreon() {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            var rep = await log.AddResultAndTransformAsync(await Statistic.GetActivePatreonAsync(db));

            if (rep == EResult.Err)
                return Ok(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>.NotExist());

            var res = new List<UsernameAndId>(rep.Ok().Count);

            foreach (var (username, id) in rep.Ok())
                res.Add(new UsernameAndId {
                    Id = id,
                    Username = username
                });

            return Ok(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>.Exist(res));
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

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class UsernameAndId {
        public string? Username { get; set; }
        public long Id { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class ActiveUser {
        public long Active1H { get; set; }
        public long Active1Day { get; set; }
        public long RegisterUser { get; set; }
    }
}
