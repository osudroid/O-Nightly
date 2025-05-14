using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public sealed class UserSettingDto: IUserSettingReadonly {
    public long UserId { get; private init; }
    public bool ShowUserClassifications { get; private init; }

    public UserSettingDto(long userId, bool showUserClassifications) {
        UserId = userId;
        ShowUserClassifications = showUserClassifications;
    }

    public static UserSettingDto From(IUserSettingReadonly userSetting) {
        return new UserSettingDto(userSetting.UserId, userSetting.ShowUserClassifications);
    }
}