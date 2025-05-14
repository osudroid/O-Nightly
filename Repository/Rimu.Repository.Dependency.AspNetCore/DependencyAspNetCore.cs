using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rimu.Repository.Dependency.Adapter;

namespace Rimu.Repository.Dependency.AspNetCore;

public static class DependencyAspNetCore {
    public static IServiceCollection AddRimuDependency(this IServiceCollection services) {
        return services;
    }
    
    public static IApplicationBuilder UseRimuDependency(this IApplicationBuilder app, List<Func<IServiceProvider, Task>> cleanUpActions)
    {
        return app.UseMiddleware<RimuDependencyMiddleware>(cleanUpActions);
    }
}