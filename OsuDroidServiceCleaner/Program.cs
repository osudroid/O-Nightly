// See https://aka.ms/new-console-template for more information

using Npgsql;
using OsuDroidLib;
using OsuDroidServiceCleaner;

namespace OsuDroidServiceCleaner;

public static class Program {
    public static async Task Main(string[] args) {
        var loadResult = (await OsuDroidLib.Setting.LoadAsync());
        if (loadResult == EResult.Err)
            throw new Exception(loadResult.Err());
        
        WriteLine("Create Cleaner Service");
        await ServiceManager<ServiceState>
            .DefaultStetting()
            .SetStateBuilder(Service.StateBuilder)
            .AddFunction(Service.RunClean)
            .ExecuteFunctionAfter(TimeSpan.FromDays(1))
            .SetFirstLoop(false)
            .Run();
    }
}