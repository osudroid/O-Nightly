using Dapper;
using LamLogger;
using Npgsql;
using OsuDroidLib.Database;
using OsuDroidLib.Extension;

namespace OsuDroidLib; 

public static class Log {
    private static bool _settingsSet = false;
    public static LamLogger.LamLog GetLog(NpgsqlConnection db) {
        
        if (_settingsSet == false) {
            LamLog.Settings = new () {
                DbTable = Setting.Log_DbName!.Value,
                LazyDbPrint = true,
                LazyTextWriterPrint = false,
                UseDbAsPrint = Setting.Log_SaveInDb!.Value,
                PrintUseOk = Setting.Log_Ok!.Value,
                PrintUseDebug = Setting.Log_Debug!.Value,
                PrintUseError = Setting.Log_Error!.Value,
                PrintDBUseOk = Setting.Log_Ok.Value,
                PrintDBUseDebug = Setting.Log_Debug.Value,
                PrintDBUseError = Setting.Log_Error.Value,
            };
        }

        async Task InsertAsync(LamLogTable[] tables) {
            foreach (var log in tables) {
                try {
                    var sql = @$"
INSERT INTO {LamLog.Settings.DbTable} 
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
        
        return new LamLog(Option<Func<LamLogTable[], Task>>.With(InsertAsync));
    }
}