using LamLogger;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib.TokenHandler;
using OsuDroid.Post;
using OsuDroidLib.Query;

namespace OsuDroid.Model; 

public static class ModelApi2Login {
    public static async Task<Result<ModelResult<ViewCreateApi2TokenResult>, string>> CreateApi2TokenAsync(
        ControllerExtensions controller, NpgsqlConnection db, LamLog log, CreateApi2TokenDto createApi2Token) {
        
        var result = await QueryUserInfo.GetIdUsernamePasswordByLowerUsernameAsync(db, createApi2Token.Username ?? "");
        if (result == EResult.Err)
            return result.ChangeOkType<ModelResult<ViewCreateApi2TokenResult>>();
        
        var userOption = result.Ok();
        
        if (userOption.IsNotSet()) {
            await log.AddLogDebugAsync("User Not Found");
            return Result<ModelResult<ViewCreateApi2TokenResult>, string>.Ok(
                ModelResult<ViewCreateApi2TokenResult>
                    .Ok(new ViewCreateApi2TokenResult {
                        Token = Guid.Empty,
                        PasswdFalse = false,
                        UsernameFalse = true
                    }));
        }
            
        var user = userOption.Unwrap();

        var passwordIsRight = OsuDroidLib.Lib.PasswordHash.IsRightPassword(createApi2Token.Passwd, user.Password!);
        if (passwordIsRight == EResult.Err)
            return Result<ModelResult<ViewCreateApi2TokenResult>, string>
                .Ok(ModelResult<ViewCreateApi2TokenResult>.Ok(new ViewCreateApi2TokenResult {
                    Token = Guid.Empty,
                    PasswdFalse = true,
                    UsernameFalse = false
                }));

        if (passwordIsRight.Ok()) {
            await log.AddLogDebugAsync("User Password False");
            return Result<ModelResult<ViewCreateApi2TokenResult>, string>
                .Ok(ModelResult<ViewCreateApi2TokenResult>.Ok(new ViewCreateApi2TokenResult {
                    Token = Guid.Empty,
                    PasswdFalse = true,
                    UsernameFalse = false
                }));
        }
        
        switch (OsuDroidLib.Lib.PasswordHash.IsBCryptHash(user.Password!)) {
            case true:
                if (!OsuDroidLib.Lib.PasswordHash.BCryptNeedRehash(user.Password!).Ok()) {
                    break;
                }
                await log.AddLogDebugAsync("Rehash Password");
                var newBcryptRehash = OsuDroidLib.Lib.PasswordHash.HashWithBCryptPassword(createApi2Token.Passwd);
                if (newBcryptRehash == EResult.Err)
                    return newBcryptRehash.ChangeOkType<ModelResult<ViewCreateApi2TokenResult>>();
                
                await OsuDroidLib.Manager.UserInfoManager
                                 .UpdatePasswordAsync(db, user.UserId, newBcryptRehash.Ok());
                break;
            default:
                var newBcrypt = OsuDroidLib.Lib.PasswordHash.HashWithBCryptPassword(createApi2Token.Passwd);
                if (newBcrypt == EResult.Err)
                    return newBcrypt.ChangeOkType<ModelResult<ViewCreateApi2TokenResult>>();

                await OsuDroidLib.Manager.UserInfoManager
                                 .UpdatePasswordAsync(db, user.UserId, newBcrypt.Ok());
                break;
        }
        
        var resultToken = await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(db, user.UserId);
            
        if (resultToken == EResult.Err) {
            return resultToken.ChangeOkType<ModelResult<ViewCreateApi2TokenResult>>();
        }

        await log.AddLogDebugAsync("Return Token");
        return Result<ModelResult<ViewCreateApi2TokenResult>, string>
            .Ok(ModelResult<ViewCreateApi2TokenResult>.Ok(new ViewCreateApi2TokenResult {
                Token = resultToken.Ok(),
                PasswdFalse = false,
                UsernameFalse = false
            }));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewWork>, string>> RefreshApi2TokenAsync(
        ControllerExtensions controller, NpgsqlConnection db, LamLog log, SimpleTokenDto simpleToken) {
        
        var tokenHandler = TokenHandlerManger.GetOrCreateCacheDatabase();
        var resultExistResult = await tokenHandler.TokenExistAsync(db, simpleToken.Token);
        if (resultExistResult == EResult.Err)
            return Result<ModelResult<ApiTypes.ViewWork>, string>.Err(resultExistResult.Err());
        
        var optionExist = resultExistResult.Ok();
        if (optionExist == false)
            return Result<ModelResult<ApiTypes.ViewWork>, string>
                .Ok(ModelResult<ApiTypes.ViewWork>.Ok(new ApiTypes.ViewWork { HasWork = false }));

            
        var resultErr = await log.AddResultAndTransformAsync<ResultErr<string>>(
            await tokenHandler.RefreshAsync(db, simpleToken.Token));;
        
        return Result<ModelResult<ApiTypes.ViewWork>, string>
            .Ok(ModelResult<ApiTypes.ViewWork>.Ok(resultErr == EResult.Err 
                ? new ApiTypes.ViewWork { HasWork = false } 
                : new ApiTypes.ViewWork { HasWork = true }));
    }
}