using LamLibAllOver;
using Npgsql;
using Npgsql.Internal;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.DbBuilder; 

public class NpgsqlCreates: IDbCreates<NpgsqlCreates.DbWrapper> {
    public async ValueTask<Result<DbWrapper, string>> Create() {
        try
        {
            var conn = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
            var transaction = await conn.BeginTransactionAsync();
            return Result<DbWrapper, string>.Ok(new DbWrapper(transaction, conn));
        }
        catch (Exception e)
        {
            return Result<DbWrapper, string>.Err(e.ToString());
        }
    }
    
    public class DbWrapper(NpgsqlTransaction transaction, NpgsqlConnection db) : IDb {
        private bool _rollback = false;
        public NpgsqlConnection Db { get; private init; } = db;

        public async ValueTask RollbackAsync() {
            if (_rollback) return;
            await transaction.RollbackAsync();
            _rollback = true;
        }

        public async ValueTask CommitAsync() {
            if (_rollback) return;
            await transaction.CommitAsync();
        }
        
        public async ValueTask DisposeAsync() {
            await transaction.DisposeAsync();
            await Db.DisposeAsync();
        }
    }
}