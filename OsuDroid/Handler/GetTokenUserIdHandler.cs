using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Class;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler; 

public class GetTokenUserIdHandler : IHandler<NpgsqlCreates.DbWrapper,LogWrapper,ControllerPostWrapper<SimpleTokenDto>,OptionHandlerOutput<long>> {
    public async ValueTask<Result<OptionHandlerOutput<long>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<SimpleTokenDto> request) {

        var db = dbWrapper.Db;
        var log = logger.Logger;
        var body = request.Post;

        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var optionResp =
            (await log.AddResultAndTransformAsync(await tokenHandler.GetTokenInfoAsync(db, body.Token))).OkOr(
                Option<TokenInfo>.Empty);

        return Result<OptionHandlerOutput<long>, string>.Ok(optionResp.IsSet()
            ? OptionHandlerOutput<long>.With(optionResp.Unwrap().UserId)
            : OptionHandlerOutput<long>.Empty
            );
    }
}