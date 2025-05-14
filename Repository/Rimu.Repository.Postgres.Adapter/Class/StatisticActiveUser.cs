namespace Rimu.Repository.Postgres.Adapter.Class;

public sealed class StatisticActiveUser {
    public long ActiveUserLast1H { get; set; }
    public long ActiveUserLast1Day { get; set; }
    public long RegisterUser { get; set; }
}