using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Authentication.Domain;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter;

namespace Rimu.Repository.Authentication.Binder;

public class ServiceAuthenticationBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<IAuthenticationProvider>((x) => new AuthenticationProvider(x.GetQueryUserInfo()));
        serviceCollection.AddScoped<IApi2TokenProvider>((x) => new Api2TokenProvider(x.GetQueryTokenUser(), x.GetEnvDb()));
    }
}