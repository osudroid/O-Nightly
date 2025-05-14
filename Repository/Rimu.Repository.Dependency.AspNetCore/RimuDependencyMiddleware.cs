using Microsoft.AspNetCore.Http;

namespace Rimu.Repository.Dependency.AspNetCore;

public class RimuDependencyMiddleware {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Func<IServiceProvider, Task>> _cleanUpActions;

    public RimuDependencyMiddleware(RequestDelegate next, List<Func<IServiceProvider, Task>> cleanUpActions, IServiceProvider serviceProvider) {
        _next = next;
        _serviceProvider = serviceProvider;
        _cleanUpActions = cleanUpActions;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception e) {
            Logger.Error(e);
        }

        foreach (var cleanUpAction in _cleanUpActions) {
            try {
                var s = cleanUpAction(_serviceProvider);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }
    }
}