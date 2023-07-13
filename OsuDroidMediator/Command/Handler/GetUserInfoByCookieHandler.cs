using System.Transactions;
using LamLibAllOver;
using Mediator;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;

namespace OsuDroidMediator.Command.Handler; 

public class GetUserInfoByCookieHandler:
    IRequestHandler<DtoRequest<OptionResponse<CookieCookieUserInfoDto>,UserInfoByCookieDto>,
                    DtoReponse<OptionResponse<CookieCookieUserInfoDto>>> {
    
    public async ValueTask<DtoReponse<OptionResponse<CookieCookieUserInfoDto>>>Handle(
        DtoRequest<OptionResponse<CookieCookieUserInfoDto>, UserInfoByCookieDto> request, CancellationToken cancellationToken) {
        var db = request.Db;
        
        var cookieOpt = request.UserCookie.GetCookie();
        if (cookieOpt.IsNotSet()) {
            return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
                Transaction<OptionResponse<CookieCookieUserInfoDto>>.Ok(OptionResponse<CookieCookieUserInfoDto>.Empty()));
        }

        var tokenHandler = OsuDroidLib.Manager.TokenHandler.TokenHandlerManger.GetOrCreateCacheDatabase();
        var userIdAndTokenResult = await tokenHandler.GetTokenInfoAsync(db, cookieOpt.Unwrap());

        
        if (userIdAndTokenResult == EResult.Err) {
            return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
                Transaction<OptionResponse<CookieCookieUserInfoDto>>.InternalServerError(userIdAndTokenResult));
        }

        if (userIdAndTokenResult.Ok().IsNotSet()) {
            return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
                Transaction<OptionResponse<CookieCookieUserInfoDto>>.Ok(OptionResponse<CookieCookieUserInfoDto>.Empty()));
        }

        var result = await OsuDroidLib.Manager.UserInfoManager
                                      .GetByUserIdAsync(db, userIdAndTokenResult.Ok().Unwrap().UserId);
        if (result == EResult.Err) {
            return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
                Transaction<OptionResponse<CookieCookieUserInfoDto>>.InternalServerError(result));
        }

        if (result.Ok().IsNotSet()) {
            return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
                Transaction<OptionResponse<CookieCookieUserInfoDto>>.InternalServerError(ResultErr<string>
                    .Err($"User With Id Not Found {userIdAndTokenResult.Ok().Unwrap().UserId}")));
        }

        UserInfo userInfo = result.Ok().Unwrap();
        return new DtoReponse<OptionResponse<CookieCookieUserInfoDto>>(
            Transaction<OptionResponse<CookieCookieUserInfoDto>>.Ok(
                OptionResponse<CookieCookieUserInfoDto>.Set(new CookieCookieUserInfoDto {
                    Id = userInfo.UserId,
                    Username = userInfo.Username??"",
                    Email = userInfo.Email??"",
                    Region = userInfo.Region??"",
                    Active = userInfo.Active,
                    Banned = userInfo.Banned,
                    RestrictMode = userInfo.RestrictMode,
                    RegistTime = userInfo.RegisterTime,
                    Supporter = userInfo.PatronEmailAccept
                })));
    }
}