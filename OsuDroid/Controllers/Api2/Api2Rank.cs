using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.Post;
using OsuDroid.Utils;
using OsuDroid.Class;
using OsuDroid.View;
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
                return await RollbackAndGetBadRequestAsync(dbT, "Values Are Bad");
            
            if (prop.HashValidate() == false)
                return await RollbackAndGetBadRequestAsync(dbT, prop.PrintHashOrder());
            
            var result = await log.AddResultAndTransformAsync(await ModelApi2Rank
                .MapFileRankAsync(this, db, DtoMapper.Api2MapFileRankToDto(prop.Body!)));
            
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

