using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class WebLoginResponseDto : IDto {
    public required bool Work { get; init; }
    public required bool UserOrPasswdOrMathIsFalse { get; init; }
    public required bool UsernameFalse { get; init; }
    public required bool EmailFalse { get; init; }
}