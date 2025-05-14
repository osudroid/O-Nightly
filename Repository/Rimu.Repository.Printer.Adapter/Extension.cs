using Microsoft.Extensions.DependencyInjection;

namespace Rimu.Repository.Printer.Adapter;

public static class Extension {
    public static IPrinterSetup GetPrinterSetup(this IServiceProvider self) => self.GetService<IPrinterSetup>() ?? throw new InvalidOperationException();
}