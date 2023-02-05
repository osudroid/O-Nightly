using Microsoft.Extensions.Configuration;

namespace OsuDroidLib;

#nullable enable

public static class Env {
    private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables()
        .Build();

    public static string OSUDROID_SECURITY_DLL {
        get {
            try {
                return Config["OSUDROID_SECURITY_DLL"] ?? "";
            }
            catch (Exception _) {
                return "";
            }
        }
    }

    public static string Domain => GetEnvironmentVariableCheckNull("DOMAIN");

    public static string AvatarPath => GetEnvironmentVariableCheckNull("AVATAR_PATH");
    public static string ReplayPath => GetEnvironmentVariableCheckNull("REPLAY_PATH");
    public static string SecurityUser => GetEnvironmentVariableCheckNull("SECURITY_USER_JSON");

    public static string PasswdSeed => GetEnvironmentVariableCheckNull("PASSWD_SEED");

    public static string? Keyword => GetEnvironmentVariableCheckNull("KEYWORD");

    public static string NoReplayEmail => GetEnvironmentVariableCheckNull("EMAIL_NO_REPLAY");

    public static string NoReplayEmailPasswd => GetEnvironmentVariableCheckNull("EMAIL_NO_REPLAY_PASSWD");

    public static string NoReplayEmailSmtpAddress => GetEnvironmentVariableCheckNull("EMAIL_NO_REPLAY_SMTP_ADDRESS");

    public static string NoReplayEmailUsername => GetEnvironmentVariableCheckNull("EMAIL_NO_REPLAY_USERNAME");

    public static string ReplayZipPath => GetEnvironmentVariableCheckNull("REPLAY_ZIP_PATH");

    public static string UpdatePath => GetEnvironmentVariableCheckNull("UPDATE_PATH");

    public static string JarPath => GetEnvironmentVariableCheckNull("JAR_PATH");
    public static bool LogPrint => bool.Parse(GetEnvironmentVariableCheckNull("LOG_PRINT"));
    public static bool LogRequestJsonPrint => bool.Parse(GetEnvironmentVariableCheckNull("LOG_REQUEST_JSON_PRINT"));

    private static string GetEnvironmentVariableCheckNull(string name) {
        return Config[name] ?? throw new NullReferenceException("EnvironmentVariable " + name);
    }
    

    #region JWT

    public static string JwtHash => GetEnvironmentVariableCheckNull("JWT_HASH");

    public static string JwtSecret => GetEnvironmentVariableCheckNull("JWT_SECRET");

    #endregion

    #region GeoIp

    public static string LicenseKeyGeoIp => GetEnvironmentVariableCheckNull("LICENSE_KEY_GEO_IP");

    public static string UserIdGeoIp => GetEnvironmentVariableCheckNull("USER_ID_GEO_IP");

    #endregion

    #region Database

    public static string CrDbIpv4 => GetEnvironmentVariableCheckNull("DB_IPV4");

    public static string CrDbPortStr => GetEnvironmentVariableCheckNull("DB_PORT");

    public static string CrDbUsername => GetEnvironmentVariableCheckNull("DB_USERNAME");

    public static string CrDbPasswd => GetEnvironmentVariableCheckNull("DB_PASSWD");

    public static string CrDbDatabase => GetEnvironmentVariableCheckNull("DATABASE");

    public static string OldDatabase => GetEnvironmentVariableCheckNull("OLD_DATABASE");

    #endregion

    #region Patreon

    public static string PatreonClientId => GetEnvironmentVariableCheckNull("PATREON_CLIENT_ID");

    public static string PatreonClientSecret => GetEnvironmentVariableCheckNull("PATREON_CLIENT_SECRET");

    public static string PatreonAccessToken => GetEnvironmentVariableCheckNull("PATREON_ACCESS_TOKEN");

    public static string PatreonRefreshToken => GetEnvironmentVariableCheckNull("PATREON_REFRESH_TOKEN");

    public static string PatreonCanpaignId => GetEnvironmentVariableCheckNull("PATREON_CAMPAIGN_ID");

    #endregion
}