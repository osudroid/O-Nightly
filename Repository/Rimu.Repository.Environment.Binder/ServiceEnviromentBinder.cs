using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Environment.Domain.Provider;
using Rimu.Repository.Postgres.Adapter;

namespace Rimu.Repository.Environment.Binder;

public class ServiceEnviromentBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<IEnvJson>((x) => x.GetEnvProvider().EnvJson);
        serviceCollection.AddScoped<IEnvDb>((x) => x.GetEnvProvider().EnvDb);
        serviceCollection.AddScoped<IEnvProvider>((x) => new EnvProvider(x.GetQuerySetting(), x.GetQuerySettingsHot()));
    }
}