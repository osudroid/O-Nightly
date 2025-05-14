using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Region.Adapter;
using Rimu.Repository.Region.Domain;
using Rimu.Repository.Environment.Adapter;

namespace Rimu.Repository.Region.Binder;

public class ServiceRegionBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<Rimu.Repository.Region.Adapter.Interface.ICountryInfoProvider>(x => new CountryInfoProvider());
        serviceCollection.AddScoped<Rimu.Repository.Region.Adapter.Interface.IIPAdressInfoProvider>(x => new IPAdressInfoProvider(x.GetEnvDb(), x.GetCountryInfoProvider()));
    }
}