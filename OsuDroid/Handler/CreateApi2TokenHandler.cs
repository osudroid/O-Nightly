using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Manager.TokenHandler;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class CreateApi2TokenHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<CreateApi2TokenDto>,
        OptionHandlerOutput<ViewCreateApi2TokenResult>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewCreateApi2TokenResult>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerPostWrapper<CreateApi2TokenDto> request) {
        var db = dbWrapper.Db;
        var createApi2Token = request.Post;
        var log = logger.Logger;

        var result = await QueryUserInfo.GetIdUsernamePasswordByLowerUsernameAsync(db, createApi2Token.Username);
        if (result == EResult.Err)
            return result.ChangeOkType<OptionHandlerOutput<ViewCreateApi2TokenResult>>();

        var userOption = result.Ok();

        if (userOption.IsNotSet()) {
            await log.AddLogDebugAsync("User Not Found");

            return Result<OptionHandlerOutput<ViewCreateApi2TokenResult>, string>.Ok(
                new OptionHandlerOutput<ViewCreateApi2TokenResult>(Option<ViewCreateApi2TokenResult>
                    .With(new ViewCreateApi2TokenResult {
                            Token = Guid.Empty,
                            PasswdFalse = false,
                            UsernameFalse = true
                        }
                    )
                )
            );
        }

        var user = userOption.Unwrap();

        var passwordIsRight = PasswordHash.IsRightPassword(createApi2Token.Password, user.Password!);
        if (passwordIsRight == EResult.Err)
            return Result<OptionHandlerOutput<ViewCreateApi2TokenResult>, string>.Ok(
                new OptionHandlerOutput<ViewCreateApi2TokenResult>(Option<ViewCreateApi2TokenResult>
                    .With(new ViewCreateApi2TokenResult {
                            Token = Guid.Empty,
                            PasswdFalse = true,
                            UsernameFalse = false
                        }
                    )
                )
            );

        if (passwordIsRight.Ok()) {
            await log.AddLogDebugAsync("User Password False");
            return Result<OptionHandlerOutput<ViewCreateApi2TokenResult>, string>.Ok(
                new OptionHandlerOutput<ViewCreateApi2TokenResult>(Option<ViewCreateApi2TokenResult>
                    .With(new ViewCreateApi2TokenResult {
                            Token = Guid.Empty,
                            PasswdFalse = true,
                            UsernameFalse = false
                        }
                    )
                )
            );
        }

        switch (PasswordHash.IsBCryptHash(user.Password!)) {
            case true:
                if (!PasswordHash.BCryptNeedRehash(user.Password!).Ok()) break;

                await log.AddLogDebugAsync("Rehash Password");
                var newBcryptRehash = PasswordHash.HashWithBCryptPassword(createApi2Token.Password);
                if (newBcryptRehash == EResult.Err)
                    return newBcryptRehash.ChangeOkType<OptionHandlerOutput<ViewCreateApi2TokenResult>>();

                await UserInfoManager
                    .UpdatePasswordAsync(db, user.UserId, newBcryptRehash.Ok());
                break;
            default:
                var newBcrypt = PasswordHash.HashWithBCryptPassword(createApi2Token.Password);
                if (newBcrypt == EResult.Err)
                    return newBcrypt.ChangeOkType<OptionHandlerOutput<ViewCreateApi2TokenResult>>();

                await UserInfoManager
                    .UpdatePasswordAsync(db, user.UserId, newBcrypt.Ok());
                break;
        }

        var resultToken = await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(db, user.UserId);

        if (resultToken == EResult.Err)
            return resultToken.ChangeOkType<OptionHandlerOutput<ViewCreateApi2TokenResult>>();

        await log.AddLogDebugAsync("Return Token");
        return Result<OptionHandlerOutput<ViewCreateApi2TokenResult>, string>.Ok(
            new OptionHandlerOutput<ViewCreateApi2TokenResult>(Option<ViewCreateApi2TokenResult>
                .With(new ViewCreateApi2TokenResult {
                        Token = resultToken.Ok(),
                        PasswdFalse = false,
                        UsernameFalse = false
                    }
                )
            )
        );
    }
}