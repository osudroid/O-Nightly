using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model.Dto;
using Riok.Mapperly.Abstractions;

namespace OsuDroidMediator; 

[Mapper]
public static partial class DtoMapper {
    public static partial CookieCookieUserInfoDto CookieUserInfoToDto(ICookieUserInfo cookieUserInfo);
    public static partial WebLoginDto CookieUserInfoToDto(IWebLogin value);
    public static partial SimpleTokenDto SimpleTokenDto(ISimpleToken value);
    public static partial UpdateAvatarDto UpdateAvatarDto(IUpdateAvatar value);
    public static partial UpdateEmailDto UpdateEmailDto(IUpdateEmail value);
    public static partial UpdatePasswordDto UpdatePasswordDto(IUpdatePassword value);
    public static partial UpdatePatreonEmailDto UpdatePatreonEmailDto(IUpdatePatreonEmail value);
    public static partial UpdateUsernameDto UpdateUsernameDto(IUpdateUsername value);
    public static partial WebLoginWithUsernameDto WebLoginWithUsernameDto(IWebLoginWithUsername value);
    public static partial WebRegisterDto WebRegisterDto(IWebRegister value);
    public static partial WebLoginDto WebLoginDto(IWebLogin value);
}