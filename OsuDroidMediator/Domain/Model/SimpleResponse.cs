using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public record SimpleResponse<T>(IDto Data) : IResponse where T : IDto;