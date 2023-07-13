using LamLibAllOver;
using LamLogger;
using Mediator;
using Npgsql;
using OsuDroidLib;
using OsuDroidLib.Database;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;

namespace OsuDroidMediator.Command.Handler; 



public class TransactionHandler<T> 
    : IRequestHandler<TransactionHandlerRequest<T>,
        TransactionHandlerResponse<T>> where T : IResponse {
    public async ValueTask<TransactionHandlerResponse<T>> Handle(TransactionHandlerRequest<T> request, CancellationToken cancellationToken) {
        var con = await DbBuilder.BuildNpgsqlConnection();
        await using var start = new  ConStart(con, await con.BeginTransactionAsync(cancellationToken), Log.GetLog(con)); 
        var (dbT, db, log) = start.Unpack();
        await log.AddLogDebugStartAsync();
        
        try {
            await log.AddLogDebugAsync("Input value Type: " + request.Data.GetType().Name);
            var mediator = Mediator.Service;
            
            var result = await mediator.Send(new ValidationHandlerRequest<T>(
                request.Data, request.UserCookie, log, db), default);
            if (await log.AddResultAndTransformAsync<ResultErr<string>>(result.Response.Err) == EResult.Err) {
                await start.DbTransaction.RollbackAsync(cancellationToken);
            }
            else {
                await start.DbTransaction.CommitAsync(default);
            }
            
            return new TransactionHandlerResponse<T>(result.Response);
        }
        catch (Exception e) {
            try { await start.DbTransaction.RollbackAsync(cancellationToken); }
            catch (Exception exception) { /* ignored */  }

            await log.AddLogErrorAsync("Exception", Option<string>.With(e.StackTrace??""));
            return new TransactionHandlerResponse<T>(Transaction<T>.InternalServerError(ResultErr<string>.Err(e.ToString())));
        }
    }
    
    private record ConStart
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
}