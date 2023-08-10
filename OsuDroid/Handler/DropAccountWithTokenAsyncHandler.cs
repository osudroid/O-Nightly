using OsuDroid.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class DropAccountWithTokenAsyncHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper,
    ControllerPostWrapper<Guid>, WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<Guid> request) {
        var db = dbWrapper.Db;
        var token = request.Post;
        var log = logger.Logger;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<WorkHandlerOutput, string>.Err(TraceMsg.WithMessage("Cookie Error"));

        await PatreonDeleteAccEmailTokenManager.RemoveToOldRows(db);


        var deleteAccTokenResponse = (await PatreonDeleteAccEmailTokenManager
            .FindByTokenWithLimitTime(db, token)).OkOrDefault();

        if (deleteAccTokenResponse.IsNotSet())
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var userId = deleteAccTokenResponse.Unwrap().UserId;
        if (userId != cookieTokenWithUserIdOption.Unwrap().UserId)
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var deleteAccountResponse = await QueryUserInfo.DeleteAsync(db, userId);
        if (deleteAccountResponse == EResult.Err)
            return deleteAccountResponse.ConvertTo<WorkHandlerOutput>();

        return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.True);
    }
}