// See https://aka.ms/new-console-template for more information

using OsuDroidLib;

namespace OsuDroidServiceRankingTimeline;

public static class Program {
    public static async Task Main(string[] args) {
        var loadResult = await Setting.LoadAsync();
        if (loadResult == EResult.Err)
            throw new Exception(loadResult.Err());

        WriteLine("Create RankingTimeline Service");

        await ServiceManager<ServiceState>
              .DefaultStetting()
              .SetStateBuilder(Service.StateBuilder)
              .AddFunction(Service.RunRankingTimeline)
              .ExecuteFunctionAfter(TimeSpan.FromHours(1))
              .SetFirstLoop(false)
              .Run();
    }
}