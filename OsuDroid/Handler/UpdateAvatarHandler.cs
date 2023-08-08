using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class UpdateAvatarHandler : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<UpdateAvatarDto>,
    OptionHandlerOutput<ViewUpdateAvatar>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewUpdateAvatar>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<UpdateAvatarDto> request) {
        var db = dbWrapper.Db;
        var cookieTokenWithUserIdOption = request.Controller.GetCookieAndUserId(db);
        var updateAvatar = request.Post;

        if (cookieTokenWithUserIdOption.IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateAvatar>, string>.Ok(OptionHandlerOutput<ViewUpdateAvatar>
                .Empty);
        var userId = cookieTokenWithUserIdOption.Unwrap().UserId;


        var userInfoResult = await QueryUserInfo.GetByUserIdAsync(db, userId);

        if (userInfoResult == EResult.Err)
            return userInfoResult.ChangeOkType<OptionHandlerOutput<ViewUpdateAvatar>>();

        if (userInfoResult.Ok().IsNotSet())
            return Result<OptionHandlerOutput<ViewUpdateAvatar>, string>
                .Err(TraceMsg.WithMessage($"userInfoResult IsNotSet UserId {userId}"));

        var userInfo = userInfoResult.Ok().Unwrap();
        var resultValidatePassword = await UserInfoManager.ValidatePasswordAndIfMd5UpdateIt(
            db, userInfo.UserId, updateAvatar.Passwd, userInfo.Password!);

        if (resultValidatePassword == EResult.Err)
            return resultValidatePassword.ChangeOkType<OptionHandlerOutput<ViewUpdateAvatar>>();

        if (!resultValidatePassword.Ok().PasswordIsValid)
            return Result<OptionHandlerOutput<ViewUpdateAvatar>, string>.Ok(OptionHandlerOutput<ViewUpdateAvatar>.With(
                new ViewUpdateAvatar {
                    PasswdFalse = true,
                    ImageToBig = false,
                    IsNotAImage = false
                }));


        var imageBytes = Array.Empty<byte>();
        try {
            var charArr = updateAvatar.ImageBase64.AsSpan(updateAvatar.ImageBase64!.IndexOf(',') + 1).ToArray();
            // TODO Write one Convert.FromBase64String With Span
            imageBytes = Convert.FromBase64CharArray(charArr, 0, charArr.Length);
        }
        catch (Exception e) {
            return Result<OptionHandlerOutput<ViewUpdateAvatar>, string>.Err(e.ToString());
        }

        var result = await UserAvatarHandler.UpdateImageForUserAsync(db, userInfo.UserId, imageBytes);

        if (result == EResult.Err)
            return result.ConvertTo<OptionHandlerOutput<ViewUpdateAvatar>>();

        return Result<OptionHandlerOutput<ViewUpdateAvatar>, string>.Ok(OptionHandlerOutput<ViewUpdateAvatar>.With(
            new ViewUpdateAvatar {
                PasswdFalse = false,
                ImageToBig = false,
                IsNotAImage = false
            }));
    }
}