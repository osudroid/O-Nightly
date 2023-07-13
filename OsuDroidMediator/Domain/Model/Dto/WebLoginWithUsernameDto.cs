using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class WebLoginWithUsernameDto : IDto {
    public required int Math { get; set; }
    public required Guid Token { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}