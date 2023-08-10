using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class CreateDropAccountTokenHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper,
    ControllerPostWrapper<CreateDropAccountTokenDto>, OptionHandlerOutput<ViewCreateDropAccountTokenRes>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<CreateDropAccountTokenDto> request) {
        var db = dbWrapper.Db;
        var createDropAccountToken = request.Post;
        var log = logger.Logger;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>.Err(
                TraceMsg.WithMessage("Cookie Error")
            );
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;


        var optionBblUserResult = await QueryUserInfo.GetByUserIdAsync(db, userId);

        if (optionBblUserResult == EResult.Err)
            return optionBblUserResult.ChangeOkType<OptionHandlerOutput<ViewCreateDropAccountTokenRes>>();

        if (optionBblUserResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>.Err(
                TraceMsg.WithMessage("User Not Found")
            );

        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, userId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<OptionHandlerOutput<ViewCreateDropAccountTokenRes>>();

        if (userInfoResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {userId}"));

        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, createDropAccountToken.Password, userInfo.Password!
        );

        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<OptionHandlerOutput<ViewCreateDropAccountTokenRes>>();

        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>.Ok(
                OptionHandlerOutput<ViewCreateDropAccountTokenRes>.With(ViewCreateDropAccountTokenRes
                    .PasswordIsFalse()
                )
            );

        var deleteAccToken = Guid.NewGuid();
        var res = await PatreonDeleteAccEmailTokenManager.InsertAsync(db, new Entities.PatreonDeleteAccEmailToken {
                UserId = userInfo.UserId,
                Email = userInfo.Email,
                CreateTime = DateTime.UtcNow,
                Token = deleteAccToken
            }
        );

        if (res == EResult.Err) return res.ConvertTo<OptionHandlerOutput<ViewCreateDropAccountTokenRes>>();
        return Result<OptionHandlerOutput<ViewCreateDropAccountTokenRes>, string>.Ok(
            OptionHandlerOutput<ViewCreateDropAccountTokenRes>.With(ViewCreateDropAccountTokenRes.NoError())
        );
    }
}