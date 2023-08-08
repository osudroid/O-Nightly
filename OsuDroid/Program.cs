using AspNetCoreRateLimit;
using Dapper;
using OsuDroid.Lib;
using OsuDroid.Utils;
using OsuDroidLib.Extension;
using OsuDroidLib.Lib;

namespace OsuDroid;

public static class Program {
    public static async Task Main(string[] args) {
        var loadResult = await Setting.LoadAsync();
        if (loadResult == EResult.Err)
            throw new Exception(loadResult.Err());

        DefaultTypeMap.MatchNamesWithUnderscores = false;
        await PrivilegeManager.Update();

        if (args.Length == 0) {
            Security.GetSecurity();
            RunWebsite();
            return;
        }

        static EExitCode ParseAndPrint(Func<Result<string, string>> fn) {
            var resp = fn();
            WriteLine(resp == EResult.Ok ? resp.Ok() : resp.Err());
            return resp == EResult.Ok ? EExitCode.Success : EExitCode.UnknownError;
        }

        static EExitCode ParseAndPrintExistCode(EExitCode code, string console) {
            WriteLine(console);
            return code;
        }

        Environment.Exit(args switch {
            ["--reload-user-stats" or "-s"] => (int)await ReloadUserStats(),
            ["--transfer"] => (int)await RunTransferDb(),
            ["--reload-timeline" or "-f"] => (int)await FullReloadRankingTimeline(),
            ["--hashpass", var password] => (int)ParseAndPrint(() =>
                PasswordHash.HashWithBCryptPassword(password)),
            _ => (int)ParseAndPrintExistCode(EExitCode.ArgNotExist, "Argument Not Exist")
        });
    }

    private static async Task<EExitCode> ReloadUserStats() {
        await new ConvertAndMoveToNewTable().RunRecalcStats();
        return EExitCode.Success;
    }

    private static async Task<EExitCode> FullReloadRankingTimeline() {
        Option<Entities.PlayScore> playScoreOption;

        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            var result = await db.SafeQueryFirstOrDefaultAsync<Entities.PlayScore>(
                "SELECT * FROM public.PlayScore ORDER BY date LIMIT 1");

            if (result == EResult.Err) {
                WriteLine(result.Err());
                return EExitCode.UnknownError;
            }

            playScoreOption = result.Ok();
        }

        if (playScoreOption.IsNotSet()) {
            WriteLine("No Score Found To Start Full Reload Ranking Timeline");
            return EExitCode.NoBblScoreFound;
        }

        var playScore = playScoreOption.Unwrap();

        FullRecalcUserRankingTimeline.Run(new DateTime(playScore.Date.Year, playScore.Date.Month, playScore.Date.Day));
        return EExitCode.Success;
    }

    private static async Task<EExitCode> RunTransferDb() {
        await new ConvertAndMoveToNewTable().OpiRun();
        return EExitCode.Success;
    }

    private static EExitCode RunWebsite() {
        RunApi(Array.Empty<string>());
        return EExitCode.Success;
    }

    private static void RunApi(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        var services = builder.Services;
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen();
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(options => {
            options.EnableEndpointRateLimiting = true;
            options.HttpStatusCode = 429;
            options.StackBlockedRequests = true;
            options.ClientIdHeader = "X-ClientId";
            options.RealIpHeader = "X-Real-IP";
            options.GeneralRules = new List<RateLimitRule> {
                new() {
                    Endpoint = "POST:/api2/play/recent",
                    Limit = 3,
                    PeriodTimespan = TimeSpan.FromSeconds(20)
                }
            };
        });


        services.AddRouting();
        services.AddCors(options => {
            options.AddPolicy("_myAllowSpecificOrigins",
                corsPolicyBuilder => {
                    corsPolicyBuilder.WithOrigins("*");
                    corsPolicyBuilder.WithHeaders("*");
                    corsPolicyBuilder.WithMethods("*");
                    corsPolicyBuilder.WithExposedHeaders("*");
                });
        });
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddInMemoryRateLimiting();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseIpRateLimiting();
        app.UseHttpsRedirection();
        // app.UseAuthorization();
        app.UsePrivilege(new List<ICookieHandler> {
            new CookieHandlerBasic()
        });
        app.MapControllers();
        app.Run();
    }
}