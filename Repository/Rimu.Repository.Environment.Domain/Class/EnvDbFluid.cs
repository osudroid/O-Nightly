using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Environment.Domain.Class;

public class EnvDbFluid: IEnvDbFluid {
    public string ChangeLogs_Path { get; set; }
    public string ChangeLogs_UpdateUrl { get; set; }
    public string ChangeLogs_Version { get; set; }
    
    public EnvDbFluid(Dictionary<string, string> dictionary) {
        ChangeLogs_Path = dictionary["ChangeLogs_Path"];
        ChangeLogs_UpdateUrl = dictionary["ChangeLogs_UpdateUrl"];
        ChangeLogs_Version = dictionary["ChangeLogs_Version"];
    }
}