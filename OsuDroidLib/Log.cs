using Dapper;
using LamLogger;
using Npgsql;
using OsuDroidLib.Extension;

namespace OsuDroidLib; 

public static class Log {
    private static bool _settingsSet = false;
    public static LamLogger.LamLog GetLog(NpgsqlConnection savePoco) {
        if (_settingsSet == false) {
            LamLogger.LamLog.Settings = new LamLogSettings() {
                DbTable = Env.LogInDbName,
                LazyDbPrint = false,
                LazyTextWriterPrint = false,
                UseDbAsPrint = Env.LogInDb,
                PrintUseOk = Env.LogOk,
                PrintUseDebug = Env.LogDebug,
                PrintUseError = Env.LogError,
                PrintDBUseOk = Env.LogOk,
                PrintDBUseDebug = Env.LogDebug,
                PrintDBUseError = Env.LogError,
            };
        }

        async Task Insert(LamLogTable[] tables) {
            var db = savePoco;

            foreach (var log in tables) {
                try {
                    var sql = @$"
INSERT INTO {Env.LogInDbName} 
    (Id, DateTime, Message, Status, Stack, Trigger)
VALUES ('{log.DateUuid.ToString()}', @DateTime, @Message, @Status, @Stack, @Trigger)
";
                    
                    await db.QueryAsync(sql, new {
                        DateTime = DateTime.SpecifyKind(log.DateTime, DateTimeKind.Utc),
                        Message = log.Message ?? "",
                        Status = log.Status.ToString(),
                        Stack = log.Stack.Or(""),
                        Trigger = log.Trigger ?? ""
                    });
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