using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Environment.Domain.Class;

public class EnvJson: IEnvJson {
    public required string ASPNETCORE_ENVIRONMENT { get; init; }
    public required string OSUDROID_SECURITY_DLL { get; init; }
    public required string DB_IPV4 { get; init; }
    public required uint DB_PORT { get; init; }
    public required string DB_USERNAME { get; init; }
    public required string DB_PASSWD { get; init; }
    public required string DATABASE { get; init; }
    public required string OLD_DATABASE { get; init; }
    public required string AVATAR_PATH { get; init; }
    public required string REPLAY_PATH { get; init; }
    public required string SECURITY_USER_JSON { get; init; }
    public required string REPLAY_ZIP_PATH { get; init; }
    public required string UPDATE_PATH { get; init; }
    public required string JAR_PATH { get; init; }
}