using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Security.Adapter;
using Rimu.Repository.Security.Domain;

namespace Rimu.Repository.Security.Binder;

public class ServiceSecurityBinder: Dependency.Adapter.Export.IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddSingleton<Adapter.Interface.ISecurityProvider>(_ => new SecurityProvider());
        serviceCollection.AddScoped<Adapter.Interface.ISecurity>(x => x.GetSecurityProvider().Security);
        serviceCollection.AddScoped<Adapter.Interface.ISecurityPhp>(x => x.GetSecurityProvider().SecurityPhp);
        serviceCollection.AddSingleton<Adapter.Interface.IInputCheckerAndConvertPhp>(x => new InputCheckerAndConvertPhp());
    }
}