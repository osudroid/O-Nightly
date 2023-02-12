using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Controllers.Api2;

public class Api2Leaderboard : ControllerExtensions {
    [HttpPost("/api2/leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetLeaderBoard([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        
        var allRegion = prop.Body!.IsRegionAll();
        Result<List<LeaderBoardUser>, string> rep;
        switch (allRegion) {
            case true:
                rep = LeaderBoard.AnyRegion(prop.Body.Limit);
                break;
            default: {
                var countyRep = prop.Body!.GetRegionAsCountry();
                if (countyRep.IsSet() == false) {
                    log.AddLogDebug("RegionAsCountry Not Found");
                    return BadRequest();
                }
                    
                rep = LeaderBoard.FilterRegion(prop.Body.Limit, countyRep.Unwrap());
                break;
            }
        }
        
        return Ok(rep == EResult.Err
            ? ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.NotExist()
            : ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>.Exist(rep.Ok()));
    }

    [HttpPost("/api2/leaderboard/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<LeaderBoardUser>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetUserLeaderBoardRank([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardUserProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        
        var rep = log.AddResultAndTransform(LeaderBoard.User(prop.Body!.UserId)).OkOr(Option<LeaderBoardUser>.Empty);

        return Ok(rep.IsSet() == false
            ? ApiTypes.ExistOrFoundInfo<LeaderBoardUser>.NotExist()
            : ApiTypes.ExistOrFoundInfo<LeaderBoardUser>.Exist(rep.Unwrap()));
    }

    [HttpPost("/api2/leaderboard/search-user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ExistOrFoundInfo<List<LeaderBoardUser>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetUserLeaderBoardRank([FromBody] ApiTypes.Api2GroundNoHeader<LeaderBoardSearchUserProp> prop) {
        if (prop.ValuesAreGood() == false)
            return BadRequest();

        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        
        ((ILogRequestJsonPrint)prop.Body!).LogRequestJsonPrint();
        
        
        
        ResultOk<List<LeaderBoardUser>> rep = prop.Body!.IsRegionAll() switch {
            true => log.AddResultAndTransform(LeaderBoard.SearchUser(prop.Body!.Limit, prop.Body!.Query!)),
            _ => log.AddResultAndTransform(LeaderBoard.SearchUserWithRegion(prop.Body!.Limit, prop.Body!.Query!,
                prop.Body.GetRegionAsCountry().Unwrap()))
        };

        return Ok(rep == EResult.Err
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
            return GetRegionAsCountry().IsSet();
        }

        public bool IsRegionAll() {
            return Region == "all";
        }

        public Option<CountryInfo.Country> GetRegionAsCountry() {
            return CountryInfo.FindByName(Region ?? "");
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class LeaderBoardSearchUserProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder, ILogRequestJsonPrint {
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

        public Option<CountryInfo.Country> GetRegionAsCountry() {
            return CountryInfo.FindByName(Region ?? "");
        }

        public bool IsRegionAll() {
            return Region == "all";
        }

        
    }
}