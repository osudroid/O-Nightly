namespace Rimu.Repository.Postgres.Adapter.Entities;

public sealed class TokenWithGroup {
    public string Group { get; set; } = "";
    public string Token { get; set; } = "";
    public DateTime CreateTime { get; set; }
    public string Data { get; set; } = "";
}