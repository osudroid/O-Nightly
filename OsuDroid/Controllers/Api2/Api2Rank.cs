using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidLib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

public class Api2Rank : ControllerExtensions {
    [HttpPost("/api2/rank/map-file")]
    [PrivilegeRoute(route: "/api2/rank/map-file")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MapFileRank([FromBody] ApiTypes.Api2GroundWithHash<Api2MapFileRankProp> prop) {
        await using var start = await GetStartAsync();
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();

        try {
            if (prop.ValuesAreGood() == false)
                return BadRequest("Values Are Bad");


            if (prop.HashValidate() == false) {
                return BadRequest(prop.PrintHashOrder());
            }



            var resultRep = await log.AddResultAndTransformAsync(await Rank
                .MapTopPlaysByFilenameAndHashAsync(
                    db,
                    prop.Body!.Filename!,
                    prop.Body!.FileHash!,
                    50
            ));

            if (resultRep == EResult.Err)
                return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>> {
                    Value = null, ExistOrFound = false
                });

            return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>> {
                Value = resultRep
                        .OkOr(Array.Empty<QueryPlayScore.MapTopPlays>()).Select(ViewMapTopPlays.FromMapTopPlays)
                        .ToList(), 
                ExistOrFound = true
            });
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

public class Api2MapFileRankProp : ApiTypes.IValuesAreGood, ApiTypes.ISingleString, ApiTypes.IPrintHashOrder {
    public string? Filename { get; set; }
    public string? FileHash { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(Filename),
            nameof(FileHash)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
            Filename??"",
            FileHash??""
        });
    }

    public bool ValuesAreGood() {
        return string.IsNullOrEmpty(Filename) != true
               && string.IsNullOrEmpty(FileHash) != true
            ;
    }
}
