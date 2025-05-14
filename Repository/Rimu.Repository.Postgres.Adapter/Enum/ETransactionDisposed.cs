namespace Rimu.Repository.Postgres.Adapter.Enum;

public enum ETransactionDisposed {
    Throw,
    Commit,
    Rollback,
}