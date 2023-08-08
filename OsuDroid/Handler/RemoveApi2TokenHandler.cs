using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class RemoveApi2TokenHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper,
    ControllerPostWrapper<SimpleTokenDto>, WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<SimpleTokenDto> request) {
        var body = request.Post;
        var db = dbWrapper.Db;
        var log = logger.Logger;

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(await tokenHandler
            .RemoveTokenAsync(db, body.Token));

        return Result<WorkHandlerOutput, string>.Ok(
            resultErr == EResult.Err
                ? WorkHandlerOutput.False
                : WorkHandlerOutput.True);
    }
}