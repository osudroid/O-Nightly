using Microsoft.Extensions.DependencyInjection;
using Rimu.Kernel;
using Rimu.Repository.Dependency.Adapter.LoadProvider;
using Rimu.Repository.Dependency.Binder;

namespace Rimu.Init.Class;

public sealed class RepositoryBinder {
    private readonly IServiceCollection? _serviceCollection;
    public LoadProvider LoadProvider { get; }

    public RepositoryBinder(IServiceCollection? serviceCollection = null) {
        this._serviceCollection = serviceCollection;
        LoadProvider = new Rimu.Repository.Dependency.Binder.LoadProvider(serviceCollection);
    }

    public ILoadProvider Bind() {
        LoadProvider.Bind(new Rimu.Repository.Environment.Binder.ServiceEnviromentBinder());
        LoadProvider.Bind(new Rimu.Repository.Postgres.Binder.ServicePostgesBinder());
        LoadProvider.Bind(new Rimu.Repository.Pp.Binder.ServicePpBinder());
        LoadProvider.Bind(new Rimu.Repository.Printer.Binder.ServicePrinterBinder());
        LoadProvider.Bind(new Rimu.Repository.Mailer.Binder.ServiceMailerBinder());
        LoadProvider.Bind(new Rimu.Repository.Security.Binder.ServiceSecurityBinder());
        LoadProvider.Bind(new Rimu.Repository.Region.Binder.ServiceRegionBinder());
        LoadProvider.Bind(new Rimu.Repository.Authentication.Binder.ServiceAuthenticationBinder());
        LoadProvider.Bind(new Rimu.Repository.Avatar.Binder.ServiceAvatarBinder());
        LoadProvider.Bind(new Rimu.Repository.OdrZip.Binder.ServiceOdrZipBinder());
        LoadProvider.Bind(x => x.AddScoped<ScopeGuid>(_ => new ScopeGuid()));
            
        return LoadProvider;
    }
}