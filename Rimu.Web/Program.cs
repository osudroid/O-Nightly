// See https://aka.ms/new-console-template for more information

using System.Reflection;
using AspNetCoreRateLimit;
using FastEndpoints;
using NLog;
using Rimu.Init;
using Rimu.Repository.Dependency.AspNetCore;
using Rimu.Repository.Postgres.Adapter;
using Logger = NLog.Logger;

namespace Rimu.Web;

public static class Program {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
    public static async Task Main(string[] args) {
        try {
            await RunApi(args);
        }
        catch (Exception) {
            await Task.Delay(TimeSpan.FromSeconds(1));
            throw;
        }
        finally {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private static async Task RunApi(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        var services = builder.Services;
        Rimu.Init.InitializeRepository.Run(services);

        services.AddInitSlice([
                typeof(Rimu.Web.Gen1.Feature.Submit.PostSubmitNew).Assembly,
                typeof(Rimu.Web.Gen2.Feature.Submit.PostSubmitPlayEnd).Assembly,
            ]
        );
        services.AddInitRepository();
        services.AddRimuDependency();
        services.AddControllers(x => {
            x.AllowEmptyInputInBodyModelBinding = true;
        });
        
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
            }
        );


        services.AddRouting();
        services.AddCors(options => {
                options.AddPolicy("_myAllowSpecificOrigins",
                    corsPolicyBuilder => {
                        corsPolicyBuilder.WithOrigins("*");
                        corsPolicyBuilder.WithHeaders("*");
                        corsPolicyBuilder.WithMethods("*");
                        corsPolicyBuilder.WithExposedHeaders("*");
                    }
                );
            }
        );
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddInMemoryRateLimiting(); ;
        services.AddFastEndpoints(x => {
            x.Assemblies = [
                typeof(Rimu.Web.Gen2.Feature.Apk.GetApkUpdateInfo).Assembly,
                typeof(Rimu.Web.Gen1.Feature.Submit.PostSubmitNew).Assembly,
            ];
        });
        services.AddResponseCaching();

        var app = builder.Build();

        app.UseInitRepository();
        app.UseRimuDependency([
            async ServiceProvider => { await ServiceProvider.GetDbTransactionContext().DisposeAsync(); },
            async ServiceProvider => { await ServiceProvider.GetDbContext().DisposeAsync(); },
        ]);
        
        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        // app.UseIpRateLimiting();
        
        // app.Use(async (context, func) => {
        //     await using AsyncServiceScope globalServiceProvider = 
        //         Rimu.Collection.Provider.CollectionHolder.GlobalServiceProvider.CreateAsyncScope();
        //     
        //     context.Items.Add("ServiceProvider", globalServiceProvider.ServiceProvider);
        //     
        //     await func();
        // });
        app.UseResponseCaching();
        app.UseFastEndpoints();
        // app.MapFastEndpoints();
        app.MapControllers();
        // Gen2.EndpointLoader.Create(app).Bind();
        
        await app.RunAsync();
    }
}