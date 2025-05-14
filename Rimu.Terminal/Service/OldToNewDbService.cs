using System.Data;
using LamLibAllOver;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Terminal.Provider;

namespace Rimu.Terminal.Service;

internal class OldToNewDbService: IDisposable {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IServiceScope _serviceScope;
    private readonly IDbContext _dbContext;
    
    private readonly CleanDbProvider _cleanDbProvider;
    private readonly InsertOldUserToNewDbProvider _insertOldUserToNewDbProvider;
    private readonly InsertOldScoresToNewDbProvider _insertOldScoresToNewDbProvider;
    
    internal OldToNewDbService(
        IServiceProvider service, 
        string pathBblScoreCsv,
        string pathBblUserCsv) {
        _serviceScope = service.CreateScope();
        _dbContext = _serviceScope.ServiceProvider.GetDbContext();
    
        var envDb = _serviceScope.ServiceProvider.GetService<IEnvDb>()!; 
        _cleanDbProvider = new CleanDbProvider(_dbContext);
        var queryUserInfo = _serviceScope.ServiceProvider.GetQueryUserInfo();
        var queryPlayStatsHistory = _serviceScope.ServiceProvider.GetQueryPlayStatsHistory();
        _insertOldUserToNewDbProvider = new InsertOldUserToNewDbProvider(
            _serviceScope.ServiceProvider.GetQueryUserInfo(),
            _serviceScope.ServiceProvider.GetQueryUserStats(),
            _serviceScope.ServiceProvider.GetQueryUserClassifications(),
            pathBblUserCsv
        );
        
        var queryReplayFile = _serviceScope.ServiceProvider.GetQueryReplayFile();
        _insertOldScoresToNewDbProvider = new InsertOldScoresToNewDbProvider(
            pathBblScoreCsv,
            queryUserInfo,
            queryPlayStatsHistory,
            queryReplayFile
        );
    }

    ~OldToNewDbService() => Dispose();

    public async Task<EResult> RunAsync() {
        using var _ = this._serviceScope;
        var db = await this._dbContext.GetConnectionAsync();
        
        try {
            await _cleanDbProvider.RunForTransferAsync();
            if (db.State != ConnectionState.Open) {
                throw new Exception("_db is not open");
            }
            // Insert UserInfo
            // Insert UserStats Default 0
            Logger.Info("Start With User");
            await _insertOldUserToNewDbProvider.RunAsync();
            Logger.Info("End   With User");
            // Insert Play
            // Insert PlayStats
            // Insert PlayStatsHistory
            Logger.Info("Start With Score");
            await _insertOldScoresToNewDbProvider.RunAsync();
            Logger.Info("End   With Score");
            return EResult.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return EResult.Err;
        }
    }

    
    
    public void Dispose() {
        this._serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }
}