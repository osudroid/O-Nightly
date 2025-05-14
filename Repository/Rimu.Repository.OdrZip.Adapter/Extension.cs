using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.OdrZip.Adapter.Interface;

namespace Rimu.Repository.OdrZip.Adapter;

public static class Extension {
    public static IOdrZip GetOdrZip(this IServiceProvider self) => self.GetService<IOdrZip>() ?? throw new InvalidOperationException();
}