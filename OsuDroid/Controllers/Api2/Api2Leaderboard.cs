using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Leaderboard : ControllerExtensions {
    [HttpPost("/api2/leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetLeaderBoard([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        var allRegion = prop.Body!.IsRegionAll();
        var rep = Response<List<LeaderBoardUser>>.Err;
        switch (allRegion) {
            case true:
                rep = LeaderBoard.AnyRegion(prop.Body.Limit);
                break;
            default: {
                var countyRep = prop.Body!.GetRegionAsCountry();
                if (countyRep == EResponse.Err)
                    return BadRequest();
                rep = LeaderBoard.FilterRegion(prop.Body.Limit, countyRep.Ok());
                break;
            }
        }

        return Ok(rep == EResponse.Err
            ? ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.NotExist()
            : ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.Exist(rep.Ok()));
    }

    [HttpPost("/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<LeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetUserLeaderBoardRank([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardUserProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        var rep = LeaderBoard.User(prop.Body!.UserId);

        return Ok(rep == EResponse.Err
            ? ApiTypes.ExistOrFoundInfo<LeaderBoardUser>.NotExist()
            : ApiTypes.ExistOrFoundInfo<LeaderBoardUser>.Exist(rep.Ok()));
    }

    [HttpPost("/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetUserLeaderBoardRank([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardSearchUserProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        var rep = prop.Body!.IsRegionAll() switch {
            true => LeaderBoard.SearchUser(prop.Body!.Limit, prop.Body!.Query!).OkOr(new()),
            _ => LeaderBoard.SearchUserWithRegion(prop.Body!.Limit, prop.Body!.Query!,
                prop.Body.GetRegionAsCountry().OkOr(new()))
        };

        return Ok(rep == EResponse.Err
            ? ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.NotExist()
            : ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.Exist(rep.OkOr(new())));
    }


    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class LeaderBoardUserProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        public long UserId { get; set; }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(UserId)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] { UserId });
        }

        public bool ValuesAreGood() {
            return UserId > -1;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class LeaderBoardProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        private string? _region;
        public int Limit { get; set; }

        public string? Region {
            get => _region;
            set => _region = value == "All" || value == "" ? "all" : value;
        }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(Limit),
                nameof(Region)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] {
                Limit,
                Region??""
            });
        }

        public bool ValuesAreGood() {
            if (Limit <= 0) return false;
            if (Region == "all") return true;
            var rep = GetRegionAsCountry();
            return rep == EResponse.Ok;
        }

        public bool IsRegionAll() {
            return Region == "all";
        }

        public Response<CountryInfo.Country> GetRegionAsCountry() {
            var res = CountryInfo.FindByName(Region ?? "");
            return res is null
                ? Response<CountryInfo.Country>.Err
                : Response<CountryInfo.Country>.Ok(res.Value);
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class LeaderBoardSearchUserProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
        private string? _region;
        public long Limit { get; set; }
        public string? Query { get; set; }

        public string Region {
            get => _region ?? "";
            set => _region = value == "All" || value == "" ? "all" : value;
        }

        public string PrintHashOrder() {
            return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(Limit),
                nameof(Query),
                nameof(Region)
            });
        }

        public string ToSingleString() {
            return Merge.ObjectsToString(new object[] {
                Limit.ToString(),
                Query ?? "",
                Region
            });
        }

        public bool ValuesAreGood() {
            return
                Limit > 0
                && !string.IsNullOrEmpty(Query);
        }

        public Response<CountryInfo.Country> GetRegionAsCountry() {
            var res = CountryInfo.FindByName(Region ?? "");
            return res is null
                ? Response<CountryInfo.Country>.Err
                : Response<CountryInfo.Country>.Ok(res.Value);
        }

        public bool IsRegionAll() {
            return Region == "all";
        }
    }
}