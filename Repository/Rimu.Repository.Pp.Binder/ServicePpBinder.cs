using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;

namespace Rimu.Repository.Pp.Binder;

public class ServicePpBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<Rimu.Repository.Pp.Adapter.IPpCalculatorContext>(_ => new Domain.PpCalculatorContext());

    }
}