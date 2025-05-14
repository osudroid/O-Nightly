using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class Setting : ISetting {
    public required string MainKey { get; set; } = "";
    public required string SubKey { get; set; } = "";
    public required string Value { get; set; } = "";

    public Setting() {
    }
}