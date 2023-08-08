using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Utils;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class ResetPasswdAndSendEmailHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<ResetPasswdAndSendEmailDto>,
        OptionHandlerOutput<ViewResetPasswdAndSendEmail>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewResetPasswdAndSendEmail>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger,
        ControllerPostWrapper<ResetPasswdAndSendEmailDto> request) {
        var controller = request.Controller;
        var prop = request.Post;
        var db = dbWrapper.Db;

        await ResetPasswordKeyManager.DeleteOldRowsAsync(db);

        var userInfoResult = (prop.Email == "") switch {
            true => await QueryUserInfo.GetByUsernameAsync(db, prop.Username),
            _ => await QueryUserInfo.GetByEmailAsync(db, prop.Email)
        };

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<OptionHandlerOutput<ViewResetPasswdAndSendEmail>>();

        var userInfoOption = userInfoResult.Ok();

        if (userInfoOption.IsNotSet())
            return Result<OptionHandlerOutput<ViewResetPasswdAndSendEmail>, string>
                .Ok(OptionHandlerOutput<ViewResetPasswdAndSendEmail>.With(
                    new ViewResetPasswdAndSendEmail { Work = false, TimeOut = false }));

        var userInfo = userInfoOption.Unwrap();
        var token = RandomText.NextAZ09(12);

        var sendResult = SendEmail.MainSendResetEmail(userInfo.UserId, userInfo.Username!, userInfo.Email!, token);
        if (sendResult == EResult.Err) return sendResult.ConvertTo<OptionHandlerOutput<ViewResetPasswdAndSendEmail>>();

        var insertResult = await ResetPasswordKeyManager.InsertAsync(db, new Entities.ResetPasswordKey {
            UserId = userInfo.UserId,
            Token = token,
            CreateTime = DateTime.UtcNow
        });

        if (insertResult == EResult.Err)
            return insertResult.ConvertTo<OptionHandlerOutput<ViewResetPasswdAndSendEmail>>();

        return Result<OptionHandlerOutput<ViewResetPasswdAndSendEmail>, string>
            .Ok(OptionHandlerOutput<ViewResetPasswdAndSendEmail>.With(new ViewResetPasswdAndSendEmail {
                Work = true,
                TimeOut = false
            }));
    }
}