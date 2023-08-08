using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;

namespace OsuDroidMediator.Application.Service; 

public static class CookieInfoService {
    public static async Task<ITransaction<OptionResponse<CookieCookieUserInfoDto>>> GetUserInfoByCookieHandlerAsync(IUserCookie controllerHandler) {
        var domainData = new DomainData<None, UserInfoByCookieDto>(None.NoneValue, _ => new UserInfoByCookieDto());
        ITransaction<OptionResponse<CookieCookieUserInfoDto>> result = await Mediator.StartNewTaskAndSendWithOptionResponse
            <OptionResponse<CookieCookieUserInfoDto>>(domainData, controllerHandler);
        return result;
    }
    
    
}