using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Command.Response; 

public record ValidationHandlerResponse<T>(ITransaction<T> Response) where T : IResponse;