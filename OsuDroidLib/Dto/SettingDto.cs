using System.Runtime.InteropServices;
using NetEscapades.EnumGenerators;
using OsuDroidLib.Class;
using OsuDroidLib;

namespace OsuDroidLib.Dto;

public record SettingDto<T>(ESettingMainKey SettingMainKey, ESettingSubKey SettingSubKey, T Value) {
    public override int GetHashCode() {
        return (EqualityComparer<ESettingMainKey>.Default.GetHashCode(SettingMainKey) * -1521134295
                + EqualityComparer<ESettingSubKey>.Default.GetHashCode(SettingSubKey)) * -1521134295;
    }



    public static Result<SettingDto<string>, string> FromSetting(Database.Entities.Setting setting) {
        if (ESettingMainKeyExtensions.TryParse(setting.MainKey, out var mainKey) == false) 
            return Result<SettingDto<string>, string>.Err(
                TraceMsg.WithMessage($"TryParse MainKey: '{setting.MainKey}'"));
        if (ESettingSubKeyExtensions.TryParse(setting.SubKey, out var subKey) == false) 
            return Result<SettingDto<string>, string>.Err(
                TraceMsg.WithMessage($"TryParse SubKey: '{setting.SubKey}'"));
        if (setting.Value is null)
            return Result<SettingDto<string>, string>.Err(TraceMsg.WithMessage($"TryParse Value is null"));
        
        return Result<SettingDto<string>, string>.Ok(new SettingDto<string>(mainKey, subKey, setting.Value));
    }
}