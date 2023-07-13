using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;

namespace OsuDroidMediator.Application.Service; 

public static class LoginService {
    public static async Task<ITransaction<OptionResponse<WebLoginResponseDto>>> WebLogin(WebLogin webLogin, IUserCookie controllerHandler) {
        var domainData = new DomainData<WebLogin, WebLoginDto>(webLogin, DtoMapper.WebLoginDto);
        var result = await Mediator.StartNewTaskAndSendWithOptionResponse
            <OptionResponse<WebLoginResponseDto>>(domainData, controllerHandler);
        return result;
    }
}