using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class RefreshApi2TokenHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<SimpleTokenDto>, WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger, ControllerPostWrapper<SimpleTokenDto> request) {
        var db = dbWrapper.Db;
        var simpleToken = request.Post;
        var log = logger.Logger;

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var resultExistResult = await tokenHandler.TokenExistAsync(db, simpleToken.Token);
        if (resultExistResult == EResult.Err)
            return resultExistResult.ChangeOkType<WorkHandlerOutput>();

        var optionExist = resultExistResult.Ok();
        if (optionExist == false)
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(
            await tokenHandler.RefreshAsync(db, simpleToken.Token));

        return Result<WorkHandlerOutput, string>.Ok(resultErr == EResult.Err
            ? WorkHandlerOutput.False
            : WorkHandlerOutput.True
        );
    }
}