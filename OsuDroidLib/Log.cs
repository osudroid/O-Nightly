using LamLogger;
using NPoco;

namespace OsuDroidLib; 

public static class Log {
    private static bool _settingsSet = false;
    public static LamLogger.LamLog GetLog(SavePoco savePoco) {
        
        if (_settingsSet == false)
            LamLogger.LamLog.Settings = new LamLogSettings() {
DbTable = Env.LogInDbName,
LazyDbPrint = false,
PrintUseDebug = Env.LogDebug,
PrintUseError = Env.LogError,
LazyTextWriterPrint = false,
UseDbAsPrint = Env.LogInDb,
PrintUseOk = Env.LogOk,
PrintDBUseDebug = Env.LogDebug,
PrintDBUseError = Env.LogError,
PrintDBUseOk = Env.LogOk
            };

        async Task Insert(LamLogTable[] tables) {
            var db = savePoco;

            foreach (var log in tables) {
                try {
                    var sql = new Sql(@$"
INSERT INTO {Env.LogInDbName} 
    (id, date_time, message, status, stack, trigger)
VALUES ('{log.DateUuid.ToString()}',@0, @1, @2, @3, @4)
",
                        DateTime.SpecifyKind(log.DateTime, DateTimeKind.Utc), 
                        log.Message??"", 
                        log.Status.ToString(), 
                        log.Stack.Or(""), 
                        log.Trigger??"");

                    await db.ExecuteAsync(sql);
                }
                catch (Exception e) {
                    WriteLine(e);
                    throw;
                }
            }
        }
        
        return new LamLog(Option<Func<LamLogTable[], Task>>.With(Insert));
    }
}