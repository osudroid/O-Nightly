using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Command.Response; 

public record TransactionHandlerResponse<T>(ITransaction<T> Response) where T : IResponse;