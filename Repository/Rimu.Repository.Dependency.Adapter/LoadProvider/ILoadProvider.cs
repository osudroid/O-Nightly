using Rimu.Repository.Dependency.Adapter.Export;

namespace Rimu.Repository.Dependency.Adapter.LoadProvider;

public interface ILoadProvider: ILoadProviderAdd<ILoadProvider> {
    public void Build();
    
    ILoadProviderAdd ILoadProviderAdd.Bind(IServiceBinder services) {
        return this.Bind(services);
    }

    public ILoadProviderAdd<ILoadProviderAdd> AsILoadProviderAdd();
}