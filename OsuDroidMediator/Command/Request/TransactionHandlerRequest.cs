using Mediator;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;

namespace OsuDroidMediator.Command.Request; 

public record TransactionHandlerRequest<TResponse>(IDomainData Data, IUserCookie UserCookie)
    : IRequest<TransactionHandlerResponse<TResponse>> 
    where TResponse : IResponse;