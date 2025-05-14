using Microsoft.Extensions.DependencyInjection;

namespace Rimu.Repository.Dependency.Adapter.Export;

public interface IServiceBinder {
    public void Bind(IServiceCollection serviceCollection);
}