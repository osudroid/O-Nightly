namespace OsuDroidAttachment.Interface;

public interface ILoggerCreates<Db, Logger>
    where Db : IDb
    where Logger: ILogger {
    public ValueTask<Logger> Create(Db db);
}