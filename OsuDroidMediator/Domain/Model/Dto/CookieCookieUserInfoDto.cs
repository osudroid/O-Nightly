using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class CookieCookieUserInfoDto: IDto, ICookieUserInfo {
    public required long Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required DateTime RegistTime { get; init; }
    public required string Region { get; init; }
    public required bool Active { get; init; }
    public required bool Supporter { get; init; }
    public required bool Banned { get; init; }
    public required bool RestrictMode { get; init; }
}