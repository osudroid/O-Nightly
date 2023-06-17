using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroidLib;
using OsuDroidLib.Query;

namespace OsuDroid.Controllers.Api2;

public class Api2Rank : ControllerExtensions {
    [HttpPost("/api2/rank/map-file")]
    [PrivilegeRoute(route: "/api2/rank/map-file")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MapFileRank([FromBody] PostApi.PostApi2GroundWithHash<PostApi2MapFileRank> prop) {
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
                return Ok(new ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>> {
                    Value = null, ExistOrFound = false
                });

            return Ok(new ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>> {
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

