using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class WebLoginDto: IDto, IWebLogin {
    public required int Math { get; init; }
    public required Guid Token { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}