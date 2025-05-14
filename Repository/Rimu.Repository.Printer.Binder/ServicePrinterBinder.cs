using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Printer.Adapter;
using Rimu.Repository.Printer.Domain;

namespace Rimu.Repository.Printer.Binder;

public class ServicePrinterBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {

        serviceCollection.AddSingleton<IPrinterSetup>(_ => new PrinterSetup());
    }
}