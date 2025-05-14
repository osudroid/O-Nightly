namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlayStatsHistoryReadonly: IPp {
    public long Id { get; }
    public long PlayId { get; }
    public long Score { get; }
    public DateTime Date { get; }
    public new double Pp { get;}
    public long? ReplayFileId { get;}
}