using LamLogger;
using Mediator;
using Npgsql;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Command.Request; 

public record ValidationHandlerRequest<TResponse>(IDomainData Data, IUserCookie UserCookie, LamLog Logger, NpgsqlConnection Db)
    : IRequest<ValidationHandlerResponse<TResponse>> 
    where TResponse : IResponse;