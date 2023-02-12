using LamLogger;

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
                await db.ExecuteAsync(@$"
INSERT INTO {Env.LogInDbName} 
    (id, date_time, message, status, stack, trigger)
VALUES (@0, @1, @2, @3, @4, @5)
", log.DateUuid.ToString(), 
                    log.DateTime, 
                    log.Message, 
                    log.Status.ToString(), 
                    log.Stack, 
                    log.Trigger
                );
            }
        }
        
        return new LamLog(Option<Func<LamLogTable[], Task>>.With(Insert));
    }
}