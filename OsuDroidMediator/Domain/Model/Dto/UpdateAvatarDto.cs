using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class UpdateAvatarDto: IDto {
    public required string ImageBase64 { get; init; }
    public required string Password { get; init; }
}