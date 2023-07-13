using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Command.Response; 

public record DtoReponse<T>(ITransaction<T> Response) where T : IResponse;