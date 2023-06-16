using System.Runtime.CompilerServices;
using OsuDroidLib.Class;
using OsuDroidLib.Dto;
using OsuDroidLib.Extension;
using OsuDroidLib.Query;

namespace OsuDroidLib.Lib; 

public static class SettingManager {
    public static async Task<ResultErr<string>> LoadAsync() {
        await using var db = await OsuDroidLib.Database.DbBuilder.BuildNpgsqlConnection();
        var result = await QuerySetting.GetAllAsync(db);
        if (result == EResult.Err)
            return result;

        var settingsResult = result.Ok().Select(SettingDto<string>.FromSetting).ToList();
        foreach (var res in settingsResult.Where(res => res == EResult.Err)) {
            return res;
        }
        var settings = settingsResult.Select(x => x.Ok()).ToList();
        
        Domain_Name = settings.First(s => s is { SettingMainKey: ESettingMainKey.Domain, SettingSubKey: ESettingSubKey.Name });  
        APK_SignKey = settings.First(s => s is { SettingMainKey: ESettingMainKey.APK, SettingSubKey: ESettingSubKey.SignKey });  
        GeoIp_LicenseKey = settings.First(s => s is { SettingMainKey: ESettingMainKey.GeoIp, SettingSubKey: ESettingSubKey.LicenseKey });  
        GeoIp_UserId = settings.First(s => s is { SettingMainKey: ESettingMainKey.GeoIp, SettingSubKey: ESettingSubKey.UserId }).ValueToInt();  
        RequestHash_Keyword = settings.First(s => s is { SettingMainKey: ESettingMainKey.RequestHash, SettingSubKey: ESettingSubKey.Keyword });  
        Password_Seed = settings.First(s => s is { SettingMainKey: ESettingMainKey.Password, SettingSubKey: ESettingSubKey.Seed });  
        Email_NoReplay = settings.First(s => s is { SettingMainKey: ESettingMainKey.Email, SettingSubKey: ESettingSubKey.NoReplay });  
        Email_NoReplayPassword = settings.First(s => s is { SettingMainKey: ESettingMainKey.Email, SettingSubKey: ESettingSubKey.NoReplayPassword });  
        Email_NoReplaySmtpAddress = settings.First(s => s is { SettingMainKey: ESettingMainKey.Email, SettingSubKey: ESettingSubKey.NoReplaySmtpAddress });  
        Email_NoReplayUsername = settings.First(s => s is { SettingMainKey: ESettingMainKey.Email, SettingSubKey: ESettingSubKey.NoReplayUsername });  
        Patreon_ClientId = settings.First(s => s is { SettingMainKey: ESettingMainKey.Patreon, SettingSubKey: ESettingSubKey.ClientId });  
        Patreon_ClientSecret = settings.First(s => s is { SettingMainKey: ESettingMainKey.Patreon, SettingSubKey: ESettingSubKey.ClientSecret });  
        Patreon_AccessToken = settings.First(s => s is { SettingMainKey: ESettingMainKey.Patreon, SettingSubKey: ESettingSubKey.AccessToken });  
        Patreon_RefreshToken = settings.First(s => s is { SettingMainKey: ESettingMainKey.Patreon, SettingSubKey: ESettingSubKey.RefreshToken });  
        Patreon_CampaignId = settings.First(s => s is { SettingMainKey: ESettingMainKey.Patreon, SettingSubKey: ESettingSubKey.CampaignId }).ValueToInt();  
        Log_DbName = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.DbName });  
        Log_SaveInDb = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.SaveInDb }).ValueToBool();  
        Log_Ok = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.Ok }).ValueToBool();  
        Log_Debug = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.Debug }).ValueToBool();  
        Log_Error = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.Error }).ValueToBool();  
        Log_RequestJsonPrint = settings.First(s => s is { SettingMainKey: ESettingMainKey.Log, SettingSubKey: ESettingSubKey.RequestJsonPrint }).ValueToBool();  
        UserAvatar_SizeLow = settings.First(s => s is { SettingMainKey: ESettingMainKey.UserAvatar, SettingSubKey: ESettingSubKey.SizeLow });  
        UserAvatar_SizeHigh = settings.First(s => s is { SettingMainKey: ESettingMainKey.UserAvatar, SettingSubKey: ESettingSubKey.SizeHigh });
        
        return ResultErr<string>.Ok();
    }

    public static SettingDto<string>? Domain_Name { get; private set; }
    public static SettingDto<string>? APK_SignKey { get; private set; }
    public static SettingDto<string>? GeoIp_LicenseKey { get; private set; }
    public static SettingDto<int>? GeoIp_UserId { get; private set; }
    public static SettingDto<string>? RequestHash_Keyword { get; private set; }
    public static SettingDto<string>? Password_Seed { get; private set; }
    public static SettingDto<string>? Email_NoReplay { get; private set; }
    public static SettingDto<string>? Email_NoReplayPassword { get; private set; }
    public static SettingDto<string>? Email_NoReplaySmtpAddress { get; private set; }
    public static SettingDto<string>? Email_NoReplayUsername { get; private set; }
    public static SettingDto<string>? Patreon_ClientId { get; private set; }
    public static SettingDto<string>? Patreon_ClientSecret { get; private set; }
    public static SettingDto<string>? Patreon_AccessToken { get; private set; }
    public static SettingDto<string>? Patreon_RefreshToken { get; private set; }
    public static SettingDto<int>? Patreon_CampaignId { get; private set; }
    public static SettingDto<string>? Log_DbName { get; private set; }
    public static SettingDto<bool>? Log_SaveInDb { get; private set; }
    public static SettingDto<bool>? Log_Ok { get; private set; }
    public static SettingDto<bool>? Log_Debug { get; private set; }
    public static SettingDto<bool>? Log_Error { get; private set; }
    public static SettingDto<bool>? Log_RequestJsonPrint { get; private set; }
    public static SettingDto<string>? UserAvatar_SizeLow { get; private set; }
    public static SettingDto<string>? UserAvatar_SizeHigh { get; private set; }
}