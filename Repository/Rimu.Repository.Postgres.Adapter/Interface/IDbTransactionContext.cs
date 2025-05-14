using System.Data;
using Rimu.Repository.Postgres.Adapter.Enum;

namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IDbTransactionContext: IDisposable, IAsyncDisposable {
    public IDbContext DbContext { get; }
    public bool IsDisposed { get; }
    public Option<IsolationLevel> IsolationLevelOption { get; }
    public bool IsTransactionActive { get; }
    public bool IsFinished { get; }
    public ETransactionDisposed DisposedOption { get; set; }
    
    public Task<ResultNone> BeginTransactionAsync();
    public Task<ResultNone> CommitAsync();
    public Task<ResultNone> RollbackAsync();
    public IDbTransactionContext SetIsolationLevel(IsolationLevel isolationLevel);
}