using Microsoft.Extensions.DependencyInjection;
using Rimu.Init.Class;
using Rimu.Kernel;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Printer.Adapter;

namespace Rimu.Init;

public static class InitializeRepository {
    public static void Run(IServiceCollection? serviceCollection = null) {
        new RepositoryBinder(serviceCollection).Bind().Build();
        new RepositorySetup().Setup();
    }
}