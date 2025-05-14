namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface ISetting {
    public string MainKey { get; }
    public string SubKey { get; }
    public string Value { get; }
}