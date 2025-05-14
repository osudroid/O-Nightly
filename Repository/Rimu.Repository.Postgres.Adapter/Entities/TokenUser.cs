
namespace Rimu.Repository.Postgres.Adapter.Entities;

public class TokenUser {
    public required string TokenId { get; set; }
    public required long UserId { get; set; }
    public required DateTime CreateDate { get; set; }

    public TokenUser() {
    }
}