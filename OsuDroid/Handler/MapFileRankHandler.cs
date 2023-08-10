using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Model;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Handler;

public class MapFileRankHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper,
    ControllerPostWrapper<Api2MapFileRankDto>, OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>> {
    public async ValueTask<Result<OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<Api2MapFileRankDto> request) {
        var db = dbWrapper.Db;
        var api2MapFileRank = request.Post;

        var resultRep = await Rank
            .MapTopPlaysByFilenameAndHashAsync(
                db,
                api2MapFileRank.Filename,
                api2MapFileRank.FileHash,
                50
            );

        if (resultRep == EResult.Err)
            return resultRep.ChangeOkType<OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>>();


        return Result<OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>, string>
            .Ok(OptionHandlerOutput<IReadOnlyList<ViewMapTopPlays>>
                .With(resultRep.Ok().Select(ViewMapTopPlays.FromMapTopPlays).ToList())
            );
    }
}