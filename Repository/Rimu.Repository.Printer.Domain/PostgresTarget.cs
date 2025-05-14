using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Common;
using NLog.Targets;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Printer.Domain;

public class PostgresTarget: AsyncTaskTarget { 
    private bool _disposed = false;
    private readonly object _connectionLock = new object();
    private readonly IQueryLog _queryLog;
    private readonly IDbContext _dbContext;
    private readonly IServiceScope _serviceScope;
    public PostgresTarget() {
        _serviceScope = Dependency.Adapter.Injection.GlobalServiceProvider.CreateScope();
        _dbContext = _serviceScope.ServiceProvider.GetDbContext();
        _queryLog = _serviceScope.ServiceProvider.GetQueryLog();
    }
    
    protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken) {
        if (this._disposed) {
            throw new ObjectDisposedException(nameof(PostgresTarget));
        }
        
        await this.EnsureNothingIsDisposedAsync();
        
        var logDto = logEvent.ToLogDto();
        await _queryLog.InsertAsync(logDto);
    }

    protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken) {
        if (this._disposed) {
            throw new ObjectDisposedException(nameof(PostgresTarget));
        }
        if (logEvents.Count == 1) {
            await WriteAsyncTask(logEvents[0], cancellationToken);
            return;
        }
        
        await this.EnsureNothingIsDisposedAsync();
        
        await _queryLog.InsertBulkAsync(logEvents.Select(x => x.ToLogDto()).ToArray());
        for (int i = 0; i < logEvents.Count; i++) {
            logEvents[i] = null!;
        }
    }

    private async ValueTask EnsureNothingIsDisposedAsync() {
        var db= await this._dbContext.GetConnectionAsync();
        
        if (_disposed) {
            throw new ObjectDisposedException(nameof(PostgresTarget));
        }
    }

    

    protected override void Dispose(bool disposing) {
        if (!this._disposed) {
            return;
        }
        
        this._disposed = true;
        this._dbContext.Dispose();
        this._serviceScope.Dispose();
        base.Dispose(disposing);
        GC.SuppressFinalize(this);
    }
}