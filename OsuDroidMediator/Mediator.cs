
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator; 

public static class Mediator {
    public static IMediator Service { get; } = GetServiceBuilder();
    
    private static IMediator GetServiceBuilder() {
        var services = new ServiceCollection(); // Most likely IServiceCollection comes from IHostBuilder/Generic host abstraction in Microsoft.Extensions.Hosting

        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        return mediator;
    }

    public static async Task<ITransaction<TResponse>> StartNewTaskAndSendWithOptionResponse<TResponse>(IDomainData data, IUserCookie userCookie, CancellationToken cancellationToken = default) 
        where TResponse : IResponse {
        
        var send = new TransactionHandlerRequest<TResponse>(data, userCookie);

        Task<TransactionHandlerResponse<TResponse>> task = Task.Factory.StartNew(async () => await Service.Send(send), TaskCreationOptions.DenyChildAttach).Unwrap();
        var result = await task.WaitAsync(cancellationToken);
        
        return result.Response;
    }
}