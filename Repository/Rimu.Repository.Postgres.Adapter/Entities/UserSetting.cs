using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class UserSetting: IUserSettingReadonly {
    public long UserId { get; set; }
    public bool ShowUserClassifications { get; set; }
}