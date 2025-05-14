namespace Rimu.Repository.Environment.Adapter.Interface;

public interface IEnvJson {
    public string ASPNETCORE_ENVIRONMENT { get; }
    public string OSUDROID_SECURITY_DLL { get; }
    public string DB_IPV4 { get; }
    public uint DB_PORT { get; }
    public string DB_USERNAME { get; }
    public string DB_PASSWD { get; }
    public string DATABASE { get; }
    public string OLD_DATABASE { get; }
    public string AVATAR_PATH { get; }
    public string REPLAY_PATH { get; }
    public string SECURITY_USER_JSON { get; }
    public string REPLAY_ZIP_PATH { get; }
    public string UPDATE_PATH { get; }
    public string JAR_PATH { get; }
}