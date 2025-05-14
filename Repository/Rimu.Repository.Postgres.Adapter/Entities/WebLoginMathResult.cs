namespace Rimu.Repository.Postgres.Adapter.Entities;

public class WebLoginMathResult {
    public required Guid WebLoginMathResultId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required int MathResult { get; set; }
}