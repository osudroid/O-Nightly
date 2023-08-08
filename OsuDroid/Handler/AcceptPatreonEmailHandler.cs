using OsuDroid.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class
    AcceptPatreonEmailHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<Guid>,
        WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger, ControllerGetWrapper<Guid> request) {
        var db = dbWrapper.Db;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);
        var token = request.Get;

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;

        var findResult = await PatreonEmailTokenManager.FindByTokenWithLimitTimeAsync(db, token);
        if (findResult == EResult.Err) return findResult.ChangeOkType<WorkHandlerOutput>();

        var findOpt = findResult.Ok();
        if (findOpt.IsNotSet()) return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var userIdAndEmail = findOpt.Unwrap();

        var resultErr = await QueryUserInfo.SetPatreonEmailAsync(db, userIdAndEmail.UserId, userIdAndEmail.Email!);
        if (resultErr == EResult.Err)
            return resultErr.ConvertTo<WorkHandlerOutput>();

        var resultSetAccept = await QueryUserInfo.SetAcceptPatreonEmailAsync(db, userIdAndEmail.UserId);
        if (resultSetAccept == EResult.Err)
            return resultSetAccept.ConvertTo<WorkHandlerOutput>();

        return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.True);
    }
}