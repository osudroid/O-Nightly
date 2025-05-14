using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Terminal.Provider;

namespace Rimu.Terminal.Service;

internal class CalcUserStatsService: IDisposable, IAsyncDisposable {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IServiceScope _serviceScope;
    private readonly IDbContext _dbContext;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly CalcUserScoreProvider _calcUserScoreProvider;

    public bool Disposed { get; private set; } = false;
    
    internal CalcUserStatsService(IServiceProvider service) {
        _serviceScope = service.CreateScope();
        _dbContext = _serviceScope.ServiceProvider.GetDbContext();
        _dbTransactionContext = _serviceScope.ServiceProvider.GetDbTransactionContext();
        _calcUserScoreProvider = new CalcUserScoreProvider(
            _serviceScope.ServiceProvider.GetQueryUserStats(),
            _serviceScope.ServiceProvider.GetQueryUserInfo(),
            _serviceScope.ServiceProvider.GetQueryView_Play_PlayStats()
        );
    }
    
    ~CalcUserStatsService() {
        Dispose();
    }

    public async Task RunAsync() {
        if (Disposed) {
            throw new ObjectDisposedException(nameof(CalcUserStatsService));
        }
        
        Logger.Info("Starting calculation UserStats service");

        
        try {
            await this._calcUserScoreProvider.RunAsync();
        }
        catch (Exception e) {
            Logger.Error(e);
            await _dbTransactionContext.RollbackAsync();
            throw;
        }
    }
    
    public void Dispose() {
        if (Disposed) {
            return;
        }
        
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync() {
        if (Disposed) {
            return;
        }
        Disposed = true;
        
        await this._dbTransactionContext.DisposeAsync();
        await this._dbContext.DisposeAsync();
        this._serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }
}