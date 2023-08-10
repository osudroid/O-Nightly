using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class UpdateUsernameHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<UpdateUsernameDto>,
        OptionHandlerOutput<ViewUpdateUsernameRes>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<UpdateUsernameDto> request) {
        var db = dbWrapper.Db;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);
        var updateUsername = request.Post;

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>.Ok(
                OptionHandlerOutput<ViewUpdateUsernameRes>.Empty
            );
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;

        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, userId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<OptionHandlerOutput<ViewUpdateUsernameRes>>();

        if (userInfoResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {userId}"));


        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateUsername.Password, userInfo.Password!
        );

        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<OptionHandlerOutput<ViewUpdateUsernameRes>>();

        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>.Ok(
                OptionHandlerOutput<ViewUpdateUsernameRes>.With(new ViewUpdateUsernameRes {
                        HasWork = false
                    }
                )
            );

        var checkUsername = await UserInfoManager.UsernameIsInUse(db, updateUsername.NewUsername);

        if (checkUsername == EResult.Err)
            return checkUsername.ChangeOkType<OptionHandlerOutput<ViewUpdateUsernameRes>>();

        if (checkUsername.Ok())
            return Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>
                .Ok(OptionHandlerOutput<ViewUpdateUsernameRes>.With(new ViewUpdateUsernameRes {
                            HasWork = false
                        }
                    )
                );

        var result = await UserInfoManager.UpdateUsernameAsync(db, userInfo.UserId, updateUsername.NewUsername);

        if (result == EResult.Err)
            return result.ConvertTo<OptionHandlerOutput<ViewUpdateUsernameRes>>();

        return Result<OptionHandlerOutput<ViewUpdateUsernameRes>, string>
            .Ok(OptionHandlerOutput<ViewUpdateUsernameRes>.With(new ViewUpdateUsernameRes { HasWork = true }));
    }
}