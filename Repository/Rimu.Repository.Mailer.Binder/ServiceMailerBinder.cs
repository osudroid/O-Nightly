using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Environment.Adapter;

namespace Rimu.Repository.Mailer.Binder;

public class ServiceMailerBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<Adapter.Interface.IMailerProvider>(x => new Domain.MailerProvider(x.GetEnvDb()));
    }
}