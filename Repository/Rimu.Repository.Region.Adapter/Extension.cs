using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Repository.Region.Adapter;

public static class Extension {
    public static Interface.ICountryInfoProvider GetCountryInfoProvider(this IServiceProvider self) => self.GetService<Interface.ICountryInfoProvider>() ?? throw new InvalidOperationException();
    public static Interface.IIPAdressInfoProvider GetIPAdressInfoProvider(this IServiceProvider self) => self.GetService<IIPAdressInfoProvider>() ?? throw new InvalidOperationException();
}