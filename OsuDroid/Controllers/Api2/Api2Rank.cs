using Microsoft.AspNetCore.Mvc;
using OsuDroid.Extensions;
using OsuDroid.Model;
using OsuDroid.Utils;
using OsuDroidLib;

namespace OsuDroid.Controllers.Api2;

public class Api2Rank : ControllerExtensions {
    [HttpPost("/api2/rank/map-file")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(ApiTypes.ExistOrFoundInfo<IReadOnlyList<MapTopPlays>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult MapFileRank([FromBody] ApiTypes.Api2GroundWithHash<Api2MapFileRankProp> prop) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        using var log = Log.GetLog(db);
        log.AddLogDebugStart();
        
        if (prop.ValuesAreGood() == false)
            return BadRequest("Values Are Bad");


        if (prop.HashValidate() == false) {
            Console.WriteLine("X");
            return BadRequest(prop.PrintHashOrder());
        }
            
        
        
        var resultRep = log.AddResultAndTransform(Rank.MapTopPlaysByFilenameAndHash(
            prop.Body!.Filename!,
            prop.Body!.FileHash!,
            50
        ));
        
        if (resultRep == EResult.Err)
            return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<MapTopPlays>> { Value = null, ExistOrFound = false });
        
        var rep = resultRep.OkOr(Array.Empty<MapTopPlays>());
        return Ok(new ApiTypes.ExistOrFoundInfo<IReadOnlyList<MapTopPlays>> { Value = rep, ExistOrFound = true });
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