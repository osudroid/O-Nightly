using NetEscapades.EnumGenerators;

namespace OsuDroidLib.Class; 

[EnumExtensions]
public enum ESettingSubKey {
    Name,
    SignKey,
    LicenseKey,
    UserId,
    Keyword,
    Seed,
    NoReplay,
    NoReplayPassword,
    NoReplaySmtpAddress,
    NoReplayUsername,
    ClientId,
    ClientSecret,
    AccessToken,
    RefreshToken,
    CampaignId,
    DbName,
    SaveInDb,
    Ok,
    Debug,
    Error,
    RequestJsonPrint,
    SizeLow,
    SizeHigh,
    MinLength,
    BCryptSalt,
}