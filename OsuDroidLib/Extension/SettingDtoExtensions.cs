using OsuDroidLib.Dto;

namespace OsuDroidLib.Extension;

public static class SettingDtoExtensions {
    public static SettingDto<int> ValueToInt(this SettingDto<string> self) {
        return new SettingDto<int>(self.SettingMainKey, self.SettingSubKey, Int32.Parse(self.Value));
    }

    public static SettingDto<bool> ValueToBool(this SettingDto<string> self) {
        return new SettingDto<bool>(self.SettingMainKey, self.SettingSubKey, Boolean.Parse(self.Value));
    }
}