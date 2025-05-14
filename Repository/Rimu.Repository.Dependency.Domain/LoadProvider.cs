using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Dependency.Adapter.LoadProvider;

namespace Rimu.Repository.Dependency.Domain;

public class LoadProvider: ILoadProvider {
    public IServiceCollection Collection { get; }
    public ILoadProvider Bind(IServiceBinder services) {
        services.Bind(Collection);
        return this;
    }

    public ILoadProvider Bind(Action<IServiceCollection> action) {
        action(this.Collection);
        return this;
    }

    public LoadProvider() => Collection = new ServiceCollection();


    public void Build() => Adapter.Injection.SetProvider(Collection.BuildServiceProvider());
    
    
    public ILoadProviderAdd<ILoadProviderAdd> AsILoadProviderAdd() => this;
    public ILoadProvider AsILoadProvider() => this;
}