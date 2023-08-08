using Npgsql;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class UpdatePatreonEmaildHandler : IHandler<NpgsqlCreates.DbWrapper,LogWrapper,ControllerPostWrapper<UpdatePatreonEmailDto>,WorkHandlerOutput> {
    public async ValueTask<Result<WorkHandlerOutput, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<UpdatePatreonEmailDto> request) {
        var db = dbWrapper.Db;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);
        var updatePatreonEmail = request.Post;
        
        if (cookieTokenWithUserIdOption.IsNotSet()) {
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);
        }
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;
        
        
        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, userId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<WorkHandlerOutput>();

        if (userInfoResult.Ok().IsNotSet()) {
            return Result<WorkHandlerOutput, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {userId}"));
        }

        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updatePatreonEmail.Passwd, userInfo.Password!);

        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<WorkHandlerOutput>();

        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.False);

        var token = Guid.NewGuid();

        var err = await OsuDroidLib.Manager.PatreonEmailTokenManager.InsertAsync(db, new Entities.PatreonEmailToken() {
            UserId = userId,
            Email = updatePatreonEmail.Email,
            CreateTime = DateTime.UtcNow,
            Token = token,
        });
        
        if (err == EResult.Err)
            return Result<WorkHandlerOutput, string>.Err(err.Err());
        
        SendEmail.MainSendPatreonVerifyLinkToken(userInfo.Username!, userInfo.Email!, token);

        return Result<WorkHandlerOutput, string>.Ok(WorkHandlerOutput.True);
    }
}