using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Class; 

public class LogCreates: ILoggerCreates<NpgsqlCreates.DbWrapper, LogWrapper> {
    public ValueTask<LogWrapper> Create(NpgsqlCreates.DbWrapper db) {
        var logWrapper = new LogWrapper(Log.GetLog(db.Db));
        return new ValueTask<LogWrapper>(logWrapper);
    }
}