using FastEndpoints;
using Microsoft.AspNetCore.Http;
namespace Rimu.Kernel;

public abstract class WebRequestHandler<TRequest, TResp> {
    protected readonly HttpContext HttpContext;

    public WebRequestHandler(HttpContext httpContext) {
        HttpContext = httpContext;
    }
    
    public TState ProcessorState<TState>() where TState : class, new() {
        return HttpContext.ProcessorState<TState>();
    }

    public abstract Task<TResp> HandleAsync(TRequest req, CancellationToken ct);
}