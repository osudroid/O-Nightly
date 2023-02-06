using AspNetCoreRateLimit;

using Npgsql;
using OsuDroid.Lib;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;

public sealed class Program {
    public static void Main(string[] args) {
        
        DbBuilder.NpgsqlConnectionString = CreateNpgsqlConnectionString();

        if (args.Length == 0) {
            Security.GetSecurity();
            RunWebsite();
            return;
        }

        static EExitCode ParseAndPrint(Func<Response<string, string>> fn) {
            var resp = fn();
            WriteLine(resp == EResponse.Ok ? resp.Ok() : resp.Err());
            return resp == EResponse.Ok ? EExitCode.Success : EExitCode.UnknownError;
        }

        static EExitCode ParseAndPrintExistCode(EExitCode code, string console) {
            WriteLine(console);
            return code;
        }

        Environment.Exit(args switch {
            ["--reload-user-stats" or "-s"] => (int)ReloadUserStats(),
            ["--transfer"] => (int)RunTransferDb(),
            ["--reload-timeline" or "-f"] => (int)FullReloadRankingTimeline(),
            ["--hashpass", var password] => (int)ParseAndPrint(() => Password.Hash(password)),
            _ => (int)ParseAndPrintExistCode(EExitCode.ArgNotExist, "Argument Not Exist")
        });
    }

    private static EExitCode ReloadUserStats() {
        new ConvertAndMoveToNewTable().Run(true);
        return EExitCode.Success;
    }
    
    private static EExitCode FullReloadRankingTimeline() {
        BblScore? s = null;
        using (var db = DbBuilder.BuildPostSqlAndOpen()) {
            s = db.First<BblScore>("SELECT * FROM public.bbl_score ORDER BY date LIMIT 1").OkOrDefault();
        }

        if (s is null) {
            WriteLine("No Score Found To Start Full Reload Ranking Timeline");
            return EExitCode.NoBblScoreFound;
        }

        FullRecalcUserRankingTimeline.Run(new DateTime(s.Date.Year, s.Date.Month, s.Date.Day));
        return EExitCode.Success;
    }

    private static EExitCode RunTransferDb() {
        new ConvertAndMoveToNewTable().Run();
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
        services.AddMemoryCache();
        services.AddControllers();
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
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
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

        app.UseIpRateLimiting();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static string CreateNpgsqlConnectionString() {
        var ip = Env.CrDbIpv4;
        var port = Convert.ToInt32(Env.CrDbPortStr);

        var connStringBuilder = new NpgsqlConnectionStringBuilder();
        connStringBuilder.Host = ip;
        connStringBuilder.Port = port;
        connStringBuilder.Password = Env.CrDbPasswd;
        connStringBuilder.Username = Env.CrDbUsername;
        connStringBuilder.Database = Env.CrDbDatabase;
        connStringBuilder.Pooling = true;
        connStringBuilder.ReadBufferSize = 1048576;
        connStringBuilder.WriteBufferSize = 1048576;
        connStringBuilder.MaxPoolSize = 1024;
        connStringBuilder.MinPoolSize = 256;
        connStringBuilder.KeepAlive = 10;
        connStringBuilder.TcpKeepAlive = true;

        return connStringBuilder.ConnectionString;
    }
}