using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Model;

namespace OsuDroid.Controllers.Api2;

public class Api2Statistic : ControllerExtensions {
    [HttpGet("/api2/statistic/active-user")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<SqlFunc.StatisticActiveUser>))]
    public IActionResult GetActiveUser() {
        var rep = Statistic.ActiveUser();
        if (rep == EResponse.Err)
            return Ok(ApiTypes.ExistOrFoundInfo<SqlFunc.StatisticActiveUser>.NotExist());
        var value = rep.Ok();
        return Ok(ApiTypes.ExistOrFoundInfo<SqlFunc.StatisticActiveUser>.Exist(new SqlFunc.StatisticActiveUser {
            RegisterUser = value.Register,
            ActiveUserLast1h = value.Last1h,
            ActiveUserLast1Day = value.Last1Day
        }));
    }

    [HttpGet("/api2/statistic/all-patreon")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>))]
    public IActionResult GetAllPatreon() {
        var rep = Statistic.GetActivePatreon();

        if (rep == EResponse.Err)
            return Ok(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>.NotExist());

        var res = new List<UsernameAndId>(rep.Ok().Count);

        foreach (var (username, id) in rep.Ok())
            res.Add(new UsernameAndId {
                Id = id,
                Username = username
            });

        return Ok(ApiTypes.ExistOrFoundInfo<List<UsernameAndId>>.Exist(res));
    }

    public sealed class UsernameAndId {
        public string? Username { get; set; }
        public long Id { get; set; }
    }

    public sealed class ActiveUser {
        public long Active1H { get; set; }
        public long Active1Day { get; set; }
        public long RegisterUser { get; set; }
    }
}