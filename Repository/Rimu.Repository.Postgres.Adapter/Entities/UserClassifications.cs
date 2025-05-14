using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class UserClassifications: IUserClassificationsReadonly {
    public long UserId { get; set; }
    public bool CoreDeveloper { get; set; }
    public bool Developer { get; set; }
    public bool Contributor { get; set; }
    public bool Supporter { get; set; }
}