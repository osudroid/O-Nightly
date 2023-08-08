using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class UpdateEmailHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<UpdateEmailDto>, WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<UpdateEmailDto> request) {
        var db = dbWrapper.Db;
        var updateEmail = request.Post;
        var cookieTokenOption = request.Controller.GetCookieAndUserId(db);
        var log = logger.Logger;

        if (cookieTokenOption.IsNotSet())
            return Result<WorkHandlerOutput, string>.Err(TraceMsg.WithMessage("Cookie Or UserId Not Found"));

        var cookieTokenAndId = cookieTokenOption.Unwrap();

        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, cookieTokenAndId.UserId);
        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<WorkHandlerOutput>();

        if (userInfoResult.Ok().IsNotSet())
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var userInfo = userInfoResult.Ok().Unwrap();

        var result = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateEmail.Passwd, userInfo.Password ?? "");

        if (result == EResult.Err)
            return Result<WorkHandlerOutput, string>.Err(result.Err());

        if (updateEmail.OldEmail != userInfo.Email)
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        switch (result.Ok()) {
            case { PasswordIsValid: false }:
                return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

            default:
                if (result.Ok().RehashPassword)
                    await log.AddLogDebugAsync($"RehashPassword For UserId: {userInfo.UserId}");
                if (result.Ok().ChangeToBCrypt)
                    await log.AddLogDebugAsync($"ChangeToBCrypt For UserId: {userInfo.UserId}");
                break;
        }

        return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.True);
    }
}