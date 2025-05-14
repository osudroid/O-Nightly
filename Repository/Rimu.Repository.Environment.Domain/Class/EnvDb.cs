using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Environment.Domain.Class;

public class EnvDb: IEnvDb {
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

    public EnvDb(Dictionary<string, string> dictionary) {
        Domain_Name = dictionary["Domain_Name"];
        APK_SignKey = dictionary["APK_SignKey"];
        GeoIp_LicenseKey = dictionary["GeoIp_LicenseKey"];
        GeoIp_UserId = int.Parse(dictionary["GeoIp_UserId"]);
        RequestHash_Keyword = dictionary["RequestHash_Keyword"];
        Password_Seed = dictionary["Password_Seed"];
        Password_MinLength = int.Parse( dictionary["Password_MinLength"]);
        Password_BCryptSalt = int.Parse(dictionary["Password_BCryptSalt"]);
        Email_NoReplay = dictionary["Email_NoReplay"];
        Email_NoReplayPassword = dictionary["Email_NoReplayPassword"];
        Email_NoReplaySmtpAddress = dictionary["Email_NoReplaySmtpAddress"];
        Email_NoReplayUsername = dictionary["Email_NoReplayUsername"];
        Log_DbName = dictionary["Log_DbName"];
        Log_SaveInDb = bool.Parse(dictionary["Log_SaveInDb"]);
        Log_Ok = bool.Parse(dictionary["Log_Ok"]);
        Log_Debug = bool.Parse(dictionary["Log_Debug"]);
        Log_Error = bool.Parse(dictionary["Log_Error"]);
        Log_RequestJsonPrint = bool.Parse(dictionary["Log_RequestJsonPrint"]);
        UserAvatar_SizeLow = int.Parse(dictionary["UserAvatar_SizeLow"]);
        UserAvatar_SizeHigh = int.Parse(dictionary["UserAvatar_SizeHigh"]);
        LoginToken_ValidTimeInMin = int.Parse(dictionary["LoginToken_ValidTimeInMin"]);
        LoginToken_TokenSize = int.Parse(dictionary["LoginToken_TokenSize"]);
        SecurityOld_Keyword = dictionary["SecurityOld_Keyword"];
        SecurityOld_AppSign = dictionary["SecurityOld_AppSign"];
        SecurityOld_AppSignDaily = dictionary["SecurityOld_AppSignDaily"];
        SecurityOld_SignKey = dictionary["SecurityOld_SignKey"];
        Security_HashValidation = bool.Parse(dictionary["Security_HashValidation"]);
        TokenUser_TTL = TimeSpan.FromSeconds(long.Parse(dictionary["TokenUser_TTL"]));
        TokenDropAccount_TTL = TimeSpan.FromSeconds(long.Parse(dictionary["TokenDropAccount_TTL"]));
        TokenResetPassword_TTL = TimeSpan.FromSeconds(long.Parse(dictionary["TokenResetPassword_TTL"]));
        TokenSignup_TTL = TimeSpan.FromSeconds(long.Parse(dictionary["TokenSignup_TTL"]));
        Pp_URL = dictionary["Pp_URL"];
    }
}