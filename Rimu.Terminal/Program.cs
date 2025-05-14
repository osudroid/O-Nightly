using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using Rimu.Terminal.Service;
using Logger = NLog.Logger;
using CommandLine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Rimu.Terminal;

public static class Program {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    public static async Task Main(string[] args) {
        Rimu.Init.InitializeRepository.Run();
        
        await CommandLine
              .Parser
              .Default
              .ParseArguments<CommandOption>(args)
              .WithParsedAsync(async option => {
                  if (!String.IsNullOrEmpty(option.UploadReplayFile)) {
                      Logger.Info($"Start UploadReplayFile in '{option.UploadReplayFile}'");
                      if (!Directory.Exists(option.UploadReplayFile)) {
                          Console.WriteLine("Directory doesn't exist");
                          System.Environment.Exit(1);
                      }

                      var insertReplayFileOdrsService = new InsertReplayFileOdrsService(option.UploadReplayFile);
                      await insertReplayFileOdrsService.RunAsync(CancellationToken.None);
                      return;
                  }
                  
                  if (option.CalcUserStats) {
                      await new CalcUserStatsService(Repository.Dependency.Adapter.Injection.GlobalServiceProvider)
                          .RunAsync();              
                  }
                  
                  if (option.Transfer) {
                      if (String.IsNullOrEmpty(option.BblScore)) {
                          Console.WriteLine("Please specify BBL Score CSV Path");
                          System.Environment.Exit(0);
                      }
                      
                      if (String.IsNullOrEmpty(option.BblUser)) {
                          Console.WriteLine("Please specify BBL User CSV Path");
                          System.Environment.Exit(0);
                      }

                      if (!System.IO.File.Exists(option.BblScore)) {
                          Console.WriteLine("Path TO BBL Score does not exist");
                          System.Environment.Exit(1);
                      }
                      
                      if (!System.IO.File.Exists(option.BblUser)) {
                          Console.WriteLine("Path TO BBL User does not exist");
                          System.Environment.Exit(1);
                      }
                      
                      await new OldToNewDbService(
                          Repository.Dependency.Adapter.Injection.GlobalServiceProvider,
                          option.BblScore,
                          option.BblUser
                      ).RunAsync();
                  }

                  if (option.GlobalTimeline) {
                      await using (var service = new Rimu.Terminal.Service.CreateNewRankingTimelineService(
                                       Repository.Dependency.Adapter.Injection.GlobalServiceProvider
                                   )) {
                          await service.RunAsync();
                      }
                  }
              });
    }
    
    private class CommandOption {
        [Option('c', "calc", Required = false, HelpText = "Calculate User Stats")]
        public bool CalcUserStats { get; set; }

        [Option('u', "upload-replay-file", Required = false, HelpText = "upload Replay File")]
        public string? UploadReplayFile { get; set; }
        [Option( "transfer", Required = false, HelpText = "Transfer Data")]
        public bool Transfer { get; set; }
        [Option( "bblscore", Required = false, HelpText = "Path To BBL Score CSV File")]
        public string? BblScore { get; set; }
        [Option( "bbluser", Required = false, HelpText = "Path To BBL User CSV File")]
        public string? BblUser { get; set; }
        [Option( 'g', "globaltimeline", Required = false, HelpText = "Path To BBL User CSV File")]
        public bool GlobalTimeline { get; set; }
    }
}