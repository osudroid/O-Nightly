using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class UpdateEmailDto : IDto {
    public required string NewEmail { get; init; }
    public required string OldEmail { get; init; }
    public required string Password { get; init; }
}