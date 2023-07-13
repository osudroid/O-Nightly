using LamLogger;
using Mediator;
using Npgsql;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Command.Request; 

public record DtoRequest<TResponse, EDto>(EDto Data, IUserCookie UserCookie, LamLog Logger, NpgsqlConnection Db): 
    IRequest<DtoReponse<TResponse>> 
    where TResponse : IResponse 
    where EDto : IDto;