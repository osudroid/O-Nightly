using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Manager.TokenHandler;

namespace OsuDroid.Handler;

public class WebLoginHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<WebLoginDto>,
        OptionHandlerOutput<ViewWebLogin>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewWebLogin>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<WebLoginDto> request) {
        var db = dbWrapper.Db;
        var data = request.Post;
        var controller = request.Controller;

        var tokenResult = await WebLoginMathResultManager.GetWebLoginTokenAsync(db, data.Token);
        if (tokenResult == EResult.Err)
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(tokenResult.ToString()!);

        if (tokenResult.Ok().IsNotSet()
            || tokenResult.Ok().Unwrap().MathResult != data.Math
           )
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Ok(OptionHandlerOutput<ViewWebLogin>.With(
                new ViewWebLogin {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }));

        var userInfoResult = await UserInfoManager.GetByEmailAsync(db, data.Email);

        if (userInfoResult == EResult.Err)
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(userInfoResult.ToString()!);

        // Email Not Found
        if (userInfoResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Err(TraceMsg.WithMessage("UserInfoId Not Found"));

        var userInfo = userInfoResult.Ok().Unwrap();

        if (!string.Equals(userInfo.Email!, data.Email, StringComparison.CurrentCultureIgnoreCase))
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Ok(OptionHandlerOutput<ViewWebLogin>.With(
                new ViewWebLogin {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }));

        var passwordValidResult = PasswordHash.IsRightPassword(data.Password, userInfo.Password ?? "");
        if (passwordValidResult == EResult.Err)
            return passwordValidResult.ChangeOkType<OptionHandlerOutput<ViewWebLogin>>();

        var passwordValid = passwordValidResult.Ok();

        if (passwordValid == false)
            return Result<OptionHandlerOutput<ViewWebLogin>, string>.Ok(OptionHandlerOutput<ViewWebLogin>.With(
                new ViewWebLogin {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }));

        if (PasswordHash.IsBCryptHash(userInfo.Password ?? "") == false) {
            var passwordHashResult = PasswordHash.HashWithBCryptPassword(data.Password);
            if (passwordHashResult == EResult.Err)
                return passwordHashResult.ChangeOkType<OptionHandlerOutput<ViewWebLogin>>();

            var resultErr = await UserInfoManager
                .UpdatePasswordAsync(db, userInfo.UserId, passwordHashResult.Ok());
            if (resultErr == EResult.Err)
                return resultErr.ToResultWithErr<OptionHandlerOutput<ViewWebLogin>>();
        }

        var userIdTokenResult =
            await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(db, userInfo.UserId);

        if (userIdTokenResult == EResult.Err)
            return userIdTokenResult.ChangeOkType<OptionHandlerOutput<ViewWebLogin>>();

        request.Controller.SetCookie(userIdTokenResult.Ok());

        return Result<OptionHandlerOutput<ViewWebLogin>, string>
            .Ok(OptionHandlerOutput<ViewWebLogin>.With(new ViewWebLogin {
                Work = true,
                EmailFalse = false,
                UsernameFalse = false,
                UserOrPasswdOrMathIsFalse = false
            }));
    }
}