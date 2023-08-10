using Dapper;
using LamLogger;
using Npgsql;

namespace OsuDroidLib;

public static class Log {
    private static readonly bool _settingsSet = false;

    public static LamLog GetLog(NpgsqlConnection db) {
        if (_settingsSet == false)
            LamLog.Settings = new LamLogSettings {
                DbTable = Setting.Log_DbName!.Value,
                LazyDbPrint = true,
                LazyTextWriterPrint = false,
                UseDbAsPrint = Setting.Log_SaveInDb!.Value,
                PrintUseOk = Setting.Log_Ok!.Value,
                PrintUseDebug = Setting.Log_Debug!.Value,
                PrintUseError = Setting.Log_Error!.Value,
                PrintDBUseOk = Setting.Log_Ok.Value,
                PrintDBUseDebug = Setting.Log_Debug.Value,
                PrintDBUseError = Setting.Log_Error.Value
            };

        async Task InsertAsync(LamLogTable[] tables) {
            for (var index = 0; index < tables.Length; index++) {
                var log = tables[index];
                try {
                    var sql = @$"
INSERT INTO {LamLog.Settings.DbTable} 
    (Id, Number, DateTime, Message, Status, Stack, Trigger)
VALUES (@Id, @Number, @DateTime, @Message, @Status, @Stack, @Trigger)
";

                    await db.QueryAsync(sql, new {
                            Id = log.DateUuid.ToGuild(),
                            Number = index,
                            DateTime = DateTime.SpecifyKind(log.DateTime, DateTimeKind.Utc),
                            Message = log.Message,
                            Status = log.Status.ToString(),
                            Stack = log.Stack.Or(""),
                            Trigger = log.Trigger
                        }
                    );
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