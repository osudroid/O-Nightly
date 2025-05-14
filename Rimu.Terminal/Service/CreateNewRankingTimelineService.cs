using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Terminal.Provider;

namespace Rimu.Terminal.Service;

internal class CreateNewRankingTimelineService: IDisposable, IAsyncDisposable {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly AsyncServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContext _dbContext;
    private readonly CleanDbProvider _cleanDbProvider;
    private readonly CreateNewRankingTimelineProvider _createNewRankingTimelineProvider;
    
    internal CreateNewRankingTimelineService(IServiceProvider service) {
        _serviceScope  = service.CreateAsyncScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dbContext = service.GetDbContext();
        _cleanDbProvider = new CleanDbProvider(_dbContext);
        var queryUserInfo = service.GetQueryUserInfo();
        var queryView_Play_PlayStatsHistory = service.GetQueryView_Play_PlayStatsHistory();

        _createNewRankingTimelineProvider = new CreateNewRankingTimelineProvider(
            queryUserInfo,
            queryView_Play_PlayStatsHistory
        );
    }

    ~CreateNewRankingTimelineService() {
        Dispose();
    }
    
    public async Task RunAsync() {
        Logger.Info("Starting Creating New RankingTimeline service");
        
        try {
            Logger.Info("Starting Cleaning");
            await _cleanDbProvider.RunForRankingTimelineAsync();
            Logger.Info("Starting NewRankingTimelineProvider");
            await _createNewRankingTimelineProvider.RunAsync();
            Logger.Info("Commting");
        }
        catch (Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }

    

    public void Dispose() {
        _serviceScope.Dispose();
        System.GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await _serviceScope.DisposeAsync();
        System.GC.SuppressFinalize(this);
    }
}