using LamLogger;
using Mediator;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.View;

namespace OsuDroid.Lib.Handler; 

record ConStart
    (NpgsqlConnection DbNormal, NpgsqlTransaction DbTransaction, LamLog LamLog) : IAsyncDisposable, IDisposable {
    public async ValueTask DisposeAsync() {
        LamLog.Dispose();
        await DbTransaction.DisposeAsync();
        await DbNormal.DisposeAsync();
    }

    public (NpgsqlTransaction dbT, NpgsqlConnection db, LamLog Log) Unpack() =>
        (DbTransaction, DbTransaction.Connection!, LamLog);

    public void Dispose() {
        DisposeAsync().AsTask().Wait();
    }
}

public class StartTransactionHandler<TView>: IRequestHandler<StartTransactionHandlerProp<TView>, StartTransactionHandlerResult<TView>> where TView: IView {
    public async ValueTask<StartTransactionHandlerResult<TView>> Handle(StartTransactionHandlerProp<TView> request, CancellationToken cancellationToken) {
        var con = await DbBuilder.BuildNpgsqlConnection();
        await using var start = new  ConStart(con, await con.BeginTransactionAsync(cancellationToken), Log.GetLog(con));    
            
        try {
            var result =await MediatorService.Mediator.Send(
                new ValidationHandlerProp<TView>(request.Value, request.Controller, start.DbNormal), cancellationToken);

            if (result.View == EResult.Err) {
                await start.DbTransaction.RollbackAsync(cancellationToken);
                
                return new StartTransactionHandlerResult<TView>(Result<ModelResult<TView>, string>.Err(result.View.Err())); 
            }

            await start.DbTransaction.CommitAsync(default);
            return new StartTransactionHandlerResult<TView>(result.View);
        }
        catch (Exception e) {
            try { await start.DbTransaction.RollbackAsync(cancellationToken); }
            catch (Exception exception) { /* ignored */  }

            return new StartTransactionHandlerResult<TView>(Result<ModelResult<TView>, string>.Err(e.ToString()));
        }
    }
}