using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Init.Class;

namespace Rimu.Init;

public static class Extension {
    public static IServiceCollection AddInitRepository(this IServiceCollection services) {
        var repositoryBinder = new RepositoryBinder();
        services.AddSingleton(repositoryBinder);
        
        return services;
    }
    
    public static IServiceCollection AddInitSlice(this IServiceCollection services, Assembly[] assemblies) {
        new InitializeSlice(services).Binds(assemblies);
        
        
        return services;
    }

    public static IApplicationBuilder UseInitRepository(this IApplicationBuilder applicationBuilder) {
        var repBinder = applicationBuilder.ApplicationServices.GetService<RepositoryBinder>() 
                        ?? throw new NullReferenceException(nameof(RepositoryBinder));
        
        repBinder.LoadProvider.SetOwnServiceProvider(applicationBuilder.ApplicationServices);
        new RepositorySetup().Setup();

        return applicationBuilder;
    }
}