using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;

namespace OsuDroid.Handler;

public class UpdatePasswordHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<UpdatePasswdDto>, WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<UpdatePasswdDto> request) {
        var db = dbWrapper.Db;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);
        var updatePasswd = request.Post;

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;

        var resultUserInfo = await UserInfoManager.GetByUserIdAsync(db, userId);
        if (resultUserInfo == EResult.Err)
            return resultUserInfo.ChangeOkType<WorkHandlerOutput>();


        var userInfoOption = resultUserInfo.Ok();
        if (userInfoOption.IsNotSet())
            return Result<WorkHandlerOutput, string>.Err(TraceMsg.WithMessage("User Not Found"));

        var userInfo = userInfoOption.Unwrap();
        var rightPassword = PasswordHash
            .IsRightPassword(updatePasswd.OldPassword, userInfo.Password ?? "");

        if (rightPassword == EResult.Err)
            return rightPassword.ChangeOkType<WorkHandlerOutput>();

        if (!rightPassword.Ok())
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var newPasswordResult = PasswordHash.HashWithBCryptPassword(updatePasswd.NewPassword);
        if (newPasswordResult == EResult.Err)
            return rightPassword.ChangeOkType<WorkHandlerOutput>();

        var result = await UserInfoManager.UpdatePasswordAsync(db, userInfo.UserId, newPasswordResult.Ok());
        if (result == EResult.Err)
            return result.ConvertTo<WorkHandlerOutput>();
        return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.True);
    }
}