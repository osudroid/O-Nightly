using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Dependency.Adapter.LoadProvider;

namespace Rimu.Repository.Dependency.Binder;

public class LoadProvider: ILoadProvider {
    public IServiceCollection Collection { get; }

    public LoadProvider() {
        Collection = new ServiceCollection();
    }
    public LoadProvider(IServiceCollection? serviceCollection ) {
        Collection = serviceCollection?? new ServiceCollection();
    }

    public ILoadProvider Bind(IServiceBinder services) {
        services.Bind(Collection);
        return this;
    }

    public ILoadProvider Bind(Action<IServiceCollection> action) {
        action(Collection);
        return this;
    }

    public void Build() => SetOwnServiceProvider(Collection.BuildServiceProvider());
    public void SetOwnServiceProvider(IServiceProvider serviceProvider) => Adapter.Injection.SetProvider(serviceProvider);
    
    
    public ILoadProviderAdd<ILoadProviderAdd> AsILoadProviderAdd() => this;
    public ILoadProvider AsILoadProvider() => this;
}