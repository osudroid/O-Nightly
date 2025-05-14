using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Postgres.Adapter;

namespace Rimu.Repository.OdrZip.Binder;

public class ServiceOdrZipBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<Adapter.Interface.IOdrZip>(x => new Domain.OdrZip(
            x.GetQueryView_Play_PlayStats(),
            x.GetQueryUserInfo(),
            x.GetQueryReplayFile()
            )
        );
    }
}