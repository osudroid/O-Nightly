namespace Rimu.Repository.Postgres.Adapter.Entities;

public class Log {
    public required DateOnly Date { get; set; }
    public required DateTime DateTime { get; set; }
    public required Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Message { get; set; }
    public required string Status { get; set; }
    public required string Stack { get; set; }
    public required string Trigger { get; set; }

    public static Log Create(DateOnly date, DateTime dateTime, string message, string status, string stack, string trigger) {
        return new Log() {
            Date = date,
            DateTime = dateTime,
            Id = Guid.CreateVersion7(),
            Message = message,
            Status = status,
            Stack = stack,
            Trigger = trigger,
        };
    }
}