using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class UpdateUsernameDto: IDto {
    public required string NewUsername { get; init; }
    public required string OldUsername { get; init; }
    public required string Password { get; init; }
}