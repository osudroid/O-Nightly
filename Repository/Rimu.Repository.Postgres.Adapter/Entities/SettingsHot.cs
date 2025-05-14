namespace Rimu.Repository.Postgres.Adapter.Entities;

public class SettingsHot {
    public required string MainKey { get; set; } = "";
    public required string SubKey { get; set; } = "";
    public required string Value { get; set; } = "";

    public SettingsHot() {
    }
}