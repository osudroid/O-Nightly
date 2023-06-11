using System.Runtime.CompilerServices;
using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Query;

namespace OsuDroidLib.Lib; 

public static class SettingHandler {
    public static async Task<Result<SettingUserAvatar, string>> GetSettingUserAvatarAsync(NpgsqlConnection db) {
        var resultE = await QuerySetting.GetSettingsAsync(db, "UserAvatar");
        if (resultE == EResult.Err)
            return resultE.ChangeOkType<SettingUserAvatar>();

        var sizeLow = -1;
        var sizeHigh = -1;
        
        foreach (var setting in resultE.Ok()) {
            switch (setting.SubKey) {
                case "SizeLow":
                    break;
                case "SizeHigh":
                    break;
                default:
                    throw new SwitchExpressionException("SettingUserAvatar SubKey Not Handel");
            }
        }
        
        return Result<SettingUserAvatar, string>.Ok(new SettingUserAvatar(sizeLow, sizeHigh));
    }
}