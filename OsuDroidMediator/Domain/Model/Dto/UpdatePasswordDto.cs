using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class UpdatePasswordDto : IDto {
    public required string NewPassword { get; init; }
    public required string OldPassword { get; init; }
}