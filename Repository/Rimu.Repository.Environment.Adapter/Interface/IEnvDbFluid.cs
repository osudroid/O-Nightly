namespace Rimu.Repository.Environment.Adapter.Interface;

public interface IEnvDbFluid {
    public string ChangeLogs_Path { get; }
    public string ChangeLogs_UpdateUrl { get; }
    public string ChangeLogs_Version { get; }
}