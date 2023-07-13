using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class WebLoginResponseDto : IDto {
    public required bool Work { get; init; }
    public required bool UserOrPasswdOrMathIsFalse { get; init; }
    public required bool UsernameExist { get; init; }
    public required bool EmailExist { get; init; }
}