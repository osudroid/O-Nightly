using LamLibAllOver;
using NLog;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Printer.Domain;

public static class Extension {
    public static Log ToLogDto(this LogEventInfo logEvent) {
        return new Log() {
            Date = DateOnly.FromDateTime(logEvent.TimeStamp),
            DateTime = logEvent.TimeStamp,
            Id = Guid.CreateVersion7(logEvent.TimeStamp),
            Message = logEvent.Message??"",
            Status = logEvent.LoggerName??"",
            Stack = logEvent.HasStackTrace? logEvent.StackTrace.ToString() :"",
            Trigger = logEvent.CallerClassName??"",
        };
    }
}