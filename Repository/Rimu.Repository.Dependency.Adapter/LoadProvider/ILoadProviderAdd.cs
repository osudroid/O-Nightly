using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;

namespace Rimu.Repository.Dependency.Adapter.LoadProvider;

public interface ILoadProviderAdd<out T> : ILoadProviderAdd where T : ILoadProviderAdd {
    public IServiceCollection Collection { get; }
    
    public new T Bind(IServiceBinder services);

    public T Bind(Action<IServiceCollection> action);
}

public interface ILoadProviderAdd {
    public ILoadProviderAdd Bind(IServiceBinder services);
}