using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Printer.Adapter;

namespace Rimu.Init.Class;

public class RepositorySetup {
    public RepositorySetup() {
    }

    public void Setup() {
        var provider = Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetEnvProvider();
        _ = provider.EnvJson;
        _ = provider.EnvDb;

        Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetPrinterSetup().Run();
    }
}