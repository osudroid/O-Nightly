using Mediator;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;
using LamLibAllOver;
using OsuDroidLib.Lib;
using OsuDroidLib.Manager;
using OsuDroidLib.Manager.TokenHandler;
using Reponse = OsuDroidMediator.Command.Response.DtoReponse<OsuDroidMediator.Domain.Model.OptionResponse<OsuDroidMediator.Domain.Model.Dto.WebLoginResponseDto>>;
using TransactionReponse = OsuDroidMediator.Domain.Model.Transaction<OsuDroidMediator.Domain.Model.OptionResponse<OsuDroidMediator.Domain.Model.Dto.WebLoginResponseDto>>;
namespace OsuDroidMediator.Command.Handler; 

public class WebLoginHandler: 
        IRequestHandler<DtoRequest<OptionResponse<WebLoginResponseDto>,WebLoginDto>,
        DtoReponse<OptionResponse<WebLoginResponseDto>>> {
    public async ValueTask<DtoReponse<OptionResponse<WebLoginResponseDto>>> Handle(
        DtoRequest<OptionResponse<WebLoginResponseDto>, WebLoginDto> request, CancellationToken cancellationToken) {

        var db = request.Db;
        var data = request.Data;

        var tokenResult = await WebLoginMathResultManager.GetWebLoginTokenAsync(db, data.Token);
        if (tokenResult == EResult.Err) {
            return new Reponse(TransactionReponse.InternalServerError(tokenResult)
            );
        }

        if (tokenResult.Ok().IsNotSet()
            || tokenResult.Ok().Unwrap().MathResult != data.Math
           ) {
            return new Reponse(
                TransactionReponse.Ok(OptionResponse<WebLoginResponseDto>.Set(new WebLoginResponseDto {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }))
            );
        }

        var userInfoResult = await UserInfoManager.GetByEmailAsync(db, data.Email);

        if (userInfoResult == EResult.Err) {
            return new DtoReponse<OptionResponse<WebLoginResponseDto>>(
                Transaction<OptionResponse<WebLoginResponseDto>>.InternalServerError(userInfoResult)
            );
        }

        // Email Not Found
        if (userInfoResult.Ok().IsNotSet()) {
            return new DtoReponse<OptionResponse<WebLoginResponseDto>>(
                Transaction<OptionResponse<WebLoginResponseDto>>.InternalServerError(
                    ResultErr<string>.Err(TraceMsg.WithMessage("UserInfoId Not Found")))
            );
        }

        var userInfo = userInfoResult.Ok().Unwrap();

        if (!string.Equals(userInfo.Email!, data.Email, StringComparison.CurrentCultureIgnoreCase)) {
            return new Reponse(TransactionReponse.Ok(
                OptionResponse<WebLoginResponseDto>.Set(new WebLoginResponseDto {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }))
            );
        }

        var passwordValidResult = PasswordHash.IsRightPassword(data.Password, userInfo.Password ?? "");
        if (passwordValidResult == EResult.Err) {
            return new Reponse(TransactionReponse.InternalServerError(passwordValidResult));
        }

        var passwordValid = passwordValidResult.Ok();

        if (passwordValid == false) {
            return new Reponse(TransactionReponse.Ok(
                OptionResponse<WebLoginResponseDto>.Set(new WebLoginResponseDto {
                    Work = false,
                    EmailFalse = false,
                    UsernameFalse = false,
                    UserOrPasswdOrMathIsFalse = true
                }))
            );
        }

        if (PasswordHash.IsBCryptHash(userInfo.Password ?? "") == false) {
            var passwordHashResult = PasswordHash.HashWithBCryptPassword(data.Password);
            if (passwordHashResult == EResult.Err)
                return new Reponse(TransactionReponse.InternalServerError(passwordHashResult));

            var resultErr = await UserInfoManager
                .UpdatePasswordAsync(request.Db, userInfo.UserId, passwordHashResult.Ok());
            if (resultErr == EResult.Err)
                return new Reponse(TransactionReponse.InternalServerError(resultErr));
        }

        var userIdTokenResult =
            await TokenHandlerManger.GetOrCreateCacheDatabase().InsertAsync(request.Db, userInfo.UserId);

        if (userIdTokenResult == EResult.Err)
            return new Reponse(TransactionReponse.InternalServerError(userIdTokenResult));

        request.UserCookie.SetCookie(userIdTokenResult.Ok());

        return new Reponse(TransactionReponse.Ok(
            OptionResponse<WebLoginResponseDto>.Set(new WebLoginResponseDto {
                Work = true,
                EmailFalse = false,
                UsernameFalse = false,
                UserOrPasswdOrMathIsFalse = false
            }))
        );
    }
}