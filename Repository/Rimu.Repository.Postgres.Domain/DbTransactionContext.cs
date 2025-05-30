using System.Transactions;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using IsolationLevel = System.Data.IsolationLevel;

namespace Rimu.Repository.Postgres.Domain;

/// <summary>
/// Represents a transaction context for managing PostgreSQL database transactions.
/// </summary>
public class DbTransactionContext: IDbTransactionContext {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    /// <summary>
    /// Gets the database context associated with this transaction context.
    /// </summary>
    public IDbContext DbContext { get; }
    /// <summary>
    /// Gets a value indicating whether the transaction context has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    /// <summary>
    /// Gets or sets the optional isolation level for the transaction.
    /// </summary>
    public Option<IsolationLevel> IsolationLevelOption { get; private set; }
    /// <summary>
    /// Gets or sets the optional Npgsql transaction associated with this context.
    /// </summary>
    public Option<NpgsqlTransaction> NpgsqlTransactionOption { get; private set; }
    /// <summary>
    /// Gets or sets the behavior when the transaction is disposed without being finished.
    /// </summary>
    public ETransactionDisposed DisposedOption { get; set; }
    /// <summary>
    /// Gets a value indicating whether a transaction is currently active.
    /// </summary>
    public bool IsTransactionActive { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the transaction has been finished (committed or rolled back).
    /// </summary>
    public bool IsFinished { get; private set; }
    
    
    
    public DbTransactionContext(IDbContext dbContext) {
        DbContext = dbContext;
        IsTransactionActive = false;
        DisposedOption = ETransactionDisposed.Throw;
        IsFinished = false;
    }

    /// <summary>
    /// Sets the isolation level for the transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to set.</param>
    /// <returns>The current transaction context.</returns>
    public IDbTransactionContext SetIsolationLevel(IsolationLevel isolationLevel) {
        IsolationLevelOption = new Option<IsolationLevel>(isolationLevel);
        return this;
    }

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
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
    
    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
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

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
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