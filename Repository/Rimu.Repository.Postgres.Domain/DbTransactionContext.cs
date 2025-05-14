using System.Transactions;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using IsolationLevel = System.Data.IsolationLevel;

namespace Rimu.Repository.Postgres.Domain;

public class DbTransactionContext: IDbTransactionContext {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public IDbContext DbContext { get; }
    public bool IsDisposed { get; private set; }
    public Option<IsolationLevel> IsolationLevelOption { get; private set; }
    public Option<NpgsqlTransaction> NpgsqlTransactionOption { get; private set; }
    public ETransactionDisposed DisposedOption { get; set; }
    
    public bool IsTransactionActive { get; private set; }
    public bool IsFinished { get; private set; }
    
    
    
    public DbTransactionContext(IDbContext dbContext) {
        DbContext = dbContext;
        IsTransactionActive = false;
        DisposedOption = ETransactionDisposed.Throw;
        IsFinished = false;
    }

    public IDbTransactionContext SetIsolationLevel(IsolationLevel isolationLevel) {
        IsolationLevelOption = new Option<IsolationLevel>(isolationLevel);
        return this;
    }

    public async Task<ResultNone> RollbackAsync() {
        try {
            if (IsDisposed) {
                throw new ObjectDisposedException(nameof(DbTransactionContext));
            }

            if (NpgsqlTransactionOption.IsNotSet()) {
                throw new NullReferenceException("No Npgsql transaction Active");
            }

            if (!IsTransactionActive) {
                throw new Exception("No transaction Active");
            }
            
            await NpgsqlTransactionOption.Unwrap().RollbackAsync();
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }
    
    public async Task<ResultNone> BeginTransactionAsync() {
        try {
            if (IsDisposed) {
                throw new ObjectDisposedException(nameof(DbTransactionContext));
            }

            if (this.IsolationLevelOption.IsNotSet()) {
                throw new NullReferenceException("No Npgsql IsolationLevelOption Is Set");
            }
            
            if (this.IsTransactionActive) {
                throw new NullReferenceException("Transaction is already active");
            }
            
            var db = await DbContext.GetConnectionAsync();
            NpgsqlTransaction transaction = await db.BeginTransactionAsync();
            NpgsqlTransactionOption = new Option<NpgsqlTransaction>(transaction);
            IsTransactionActive = false;
            
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }

    public async Task<ResultNone> CommitAsync() {
        try {
            if (IsDisposed) {
                throw new ObjectDisposedException(nameof(DbTransactionContext));
            }

            if (NpgsqlTransactionOption.IsNotSet()) {
                throw new NullReferenceException("No Npgsql transaction Active");
            }

            if (!IsTransactionActive) {
                throw new Exception("No transaction Active");
            }
            
            await NpgsqlTransactionOption.Unwrap().CommitAsync();
            
            return ResultNone.Ok;
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultNone.Err;
        }
    }


    public void Dispose() {
#pragma warning disable CA2012
        DisposeAsync(false);
#pragma warning restore CA2012
    }

    public ValueTask DisposeAsync() {
        return DisposeAsync(true);
    }
    
    public ValueTask DisposeAsync(bool isAsync) {
        if (IsDisposed) {
            return ValueTask.CompletedTask;
        }
        
        IsDisposed = true;
        if (this.NpgsqlTransactionOption.IsNotSet()) {
            return ValueTask.CompletedTask;
        }

        
        if (!this.IsFinished || isAsync) {
            return InAsync();
        }
        
        NpgsqlTransactionOption.Unwrap().Dispose();
        
        return ValueTask.CompletedTask;

        async ValueTask InAsync() {
            if (!this.IsFinished) {
                await RunDisposedOptionAsync();
            }   
            
            await NpgsqlTransactionOption.Unwrap().DisposeAsync();
        }
        
        async ValueTask RunDisposedOptionAsync() {
            switch (DisposedOption) {
                case ETransactionDisposed.Throw:
                    throw new TransactionException("Transaction is not finished");
                case ETransactionDisposed.Commit:
                    await this.CommitAsync();
                    break;
                case ETransactionDisposed.Rollback:
                    await this.RollbackAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}