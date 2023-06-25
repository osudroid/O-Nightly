using Npgsql;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.View;
using OsuDroidLib.Query;

namespace OsuDroid.Model; 

public static class ModelApi2Rank {
    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>>, string>> MapFileRankAsync(
        ControllerExtensions controller, NpgsqlConnection db, Api2MapFileRankDto api2MapFileRank) {
        
        var resultRep = await Rank
            .MapTopPlaysByFilenameAndHashAsync(
                db,
                api2MapFileRank.Filename!,
                api2MapFileRank.FileHash!,
                50
            );

        if (resultRep == EResult.Err)
            return resultRep.ChangeOkType<ModelResult<ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>>>();


        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>>, string>
            .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>>.Ok(ApiTypes.ViewExistOrFoundInfo<IReadOnlyList<ViewMapTopPlays>>
                .Exist(resultRep.Map<IReadOnlyList<ViewMapTopPlays>>(x 
                    => x.Select(ViewMapTopPlays.FromMapTopPlays).ToList()).Ok())));
    }
}