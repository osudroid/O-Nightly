// See https://aka.ms/new-console-template for more information

using Npgsql;
using OsuDroidLib;

namespace OsuDroidServicePatreon;

public static class Program {
    public static async Task Main(string[] args) {
        var loadResult = (await OsuDroidLib.Setting.LoadAsync());
        if (loadResult == EResult.Err)
            throw new Exception(loadResult.Err());

        WriteLine("Create Patreon Service");

        await ServiceManager<ServiceState>
            .DefaultStetting()
            .SetStateBuilder(Service.StateBuilder)
            .AddFunction(Service.RunClean)
            .ExecuteFunctionAfter(TimeSpan.FromHours(1))
            .SetFirstLoop(false)
            .Run();
    }
}