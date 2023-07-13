using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class UpdatePatreonEmailDto: IDto {
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Username { get; init; }
}