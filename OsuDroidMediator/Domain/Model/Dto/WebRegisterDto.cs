using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class WebRegisterDto : IDto {
    public required string Email { get; init; }
    public required int MathResult { get; init; }
    public required Guid MathToken { get; init; }
    public required string Region { get; init; }
    public required string Password { get; init; }
    public required string Username { get; init; }
}