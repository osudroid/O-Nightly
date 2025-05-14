using Microsoft.Extensions.Configuration;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Environment.Domain.Class;

namespace Rimu.Repository.Environment.Domain.Provider;

internal  class EnvJsonProviderBehavior: IProviderBehavior<IEnvJson> {
    private static volatile IEnvJson? _envJson;

    public IEnvJson GetInstance {
        get {
            if (_envJson is null) {
                _envJson = Create();
            }
            return _envJson;
        }
    }

    public EnvJsonProviderBehavior() { }
    

    private static IEnvJson Create() {
        var config = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", true)
                     .AddEnvironmentVariables()
                     .Build();

        return new EnvJson {
            ASPNETCORE_ENVIRONMENT = GetEnvironmentVariableCheckNull(config, "ASPNETCORE_ENVIRONMENT"),
            OSUDROID_SECURITY_DLL = GetEnvironmentVariableCheckNull(config, "OSUDROID_SECURITY_DLL"),
            DB_IPV4 = GetEnvironmentVariableCheckNull(config, "DB_IPV4"),
            DB_PORT = uint.Parse(GetEnvironmentVariableCheckNull(config, "DB_PORT")),
            DB_USERNAME = GetEnvironmentVariableCheckNull(config, "DB_USERNAME"),
            DB_PASSWD = GetEnvironmentVariableCheckNull(config, "DB_PASSWD"),
            DATABASE = GetEnvironmentVariableCheckNull(config, "DATABASE"),
            OLD_DATABASE = GetEnvironmentVariableCheckNull(config, "OLD_DATABASE"),
            AVATAR_PATH = GetEnvironmentVariableCheckNull(config, "AVATAR_PATH"),
            REPLAY_PATH = GetEnvironmentVariableCheckNull(config, "REPLAY_PATH"),
            SECURITY_USER_JSON = GetEnvironmentVariableCheckNull(config, "SECURITY_USER_JSON"),
            REPLAY_ZIP_PATH = GetEnvironmentVariableCheckNull(config, "REPLAY_ZIP_PATH"),
            UPDATE_PATH = GetEnvironmentVariableCheckNull(config, "UPDATE_PATH"),
            JAR_PATH = GetEnvironmentVariableCheckNull(config, "JAR_PATH"),
        };
    }
    
    private static string GetEnvironmentVariableCheckNull(IConfigurationRoot config, string name) {
        return config[name] ?? throw new NullReferenceException("EnvironmentVariable " + name);
    }
}