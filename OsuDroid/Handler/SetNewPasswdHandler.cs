using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;

namespace OsuDroid.Handler;

public class SetNewPasswdHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<SetNewPasswdDto>,
        OptionHandlerOutput<ViewWebReplacePasswordWithToken>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewWebReplacePasswordWithToken>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<SetNewPasswdDto> request) {
        var db = dbWrapper.Db;
        var prop = request.Post;

        await ResetPasswordKeyManager.DeleteOldRowsAsync(db);
        var found = await ResetPasswordKeyManager.FindActiveKeyByTokenAsync(db, prop.Token, prop.UserId);

        if (found == EResult.Err)
            return found.ChangeOkType<OptionHandlerOutput<ViewWebReplacePasswordWithToken>>();

        if (found.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewWebReplacePasswordWithToken>, string>.Ok(
                OptionHandlerOutput<ViewWebReplacePasswordWithToken>.With(new ViewWebReplacePasswordWithToken {
                        Work = false,
                        ErrorMsg = "Token To Old"
                    }
                )
            );

        var key = found.Ok().Unwrap();

        var hashPasswordResult = PasswordHash.HashWithBCryptPassword(prop.NewPassword);
        if (hashPasswordResult == EResult.Err)
            return found.ChangeOkType<OptionHandlerOutput<ViewWebReplacePasswordWithToken>>();

        var hashPassword = hashPasswordResult.Ok();
        var updateResult = await UserInfoManager.UpdatePasswordAsync(db, key.UserId, hashPassword);
        if (updateResult == EResult.Err)
            return updateResult.ConvertTo<OptionHandlerOutput<ViewWebReplacePasswordWithToken>>();

        return Result<OptionHandlerOutput<ViewWebReplacePasswordWithToken>, string>
            .Ok(OptionHandlerOutput<ViewWebReplacePasswordWithToken>.With(new ViewWebReplacePasswordWithToken {
                        Work = true,
                        ErrorMsg = ""
                    }
                )
            );
    }
}