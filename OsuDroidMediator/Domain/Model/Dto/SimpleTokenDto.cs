using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model.Dto; 

public class SimpleTokenDto: IDto {
    public required Guid Token { get; set; }
}