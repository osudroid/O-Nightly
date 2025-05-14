namespace Rimu.Repository.Environment.Adapter.Interface;

public interface IEnvDb {
    public string Domain_Name { get; }
    public string APK_SignKey { get; }
    public string GeoIp_LicenseKey { get; }
    public int GeoIp_UserId { get; }
    public string RequestHash_Keyword { get; }
    public string Password_Seed { get; }
    public int Password_MinLength { get; }
    public int Password_BCryptSalt { get; }
    public string Email_NoReplay { get; }
    public string Email_NoReplayPassword { get; }
    public string Email_NoReplaySmtpAddress { get; }
    public string Email_NoReplayUsername { get; }
    public string Log_DbName { get; }
    public bool Log_SaveInDb { get; }
    public bool Log_Ok { get; }
    public bool Log_Debug { get; }
    public bool Log_Error { get; }
    public bool Log_RequestJsonPrint { get; }
    public int UserAvatar_SizeLow { get; }
    public int UserAvatar_SizeHigh { get; }
    public int LoginToken_ValidTimeInMin { get; }
    public int LoginToken_TokenSize { get; }
    public string SecurityOld_Keyword { get; }
    public string SecurityOld_AppSign { get; }
    public string SecurityOld_AppSignDaily { get; }
    public string SecurityOld_SignKey { get; }
    public bool Security_HashValidation { get; }
    public TimeSpan TokenUser_TTL { get; }
    public TimeSpan TokenDropAccount_TTL { get; }
    public TimeSpan TokenResetPassword_TTL { get; }
    public TimeSpan TokenSignup_TTL { get; }
    public string Pp_URL { get; }
}