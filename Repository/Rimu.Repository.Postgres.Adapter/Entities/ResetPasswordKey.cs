namespace Rimu.Repository.Postgres.Adapter.Entities;

public class ResetPasswordKey {
    public required string? Token { get; set; }
    public required long UserId { get; set; }
    public required DateTime CreateTime { get; set; }

    public ResetPasswordKey() {
    }
}