using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public class UserClassificationsDto: IUserClassificationsReadonly {
    public long UserId { get; }
    public bool CoreDeveloper { get; }
    public bool Developer { get; }
    public bool Contributor { get; }
    public bool Supporter { get; }

    public UserClassificationsDto(IUserClassificationsReadonly userClassificationsReadonly) {
        UserId = userClassificationsReadonly.UserId;
        CoreDeveloper = userClassificationsReadonly.CoreDeveloper;
        Developer = userClassificationsReadonly.Developer;
        Contributor = userClassificationsReadonly.Contributor;
        Supporter = userClassificationsReadonly.Supporter;
    }
    
    public UserClassificationsDto(long userId, bool coreDeveloper, bool developer, bool contributor, bool supporter) {
        UserId = userId;
        CoreDeveloper = coreDeveloper;
        Developer = developer;
        Contributor = contributor;
        Supporter = supporter;
    }
    
    public static UserClassificationsDto From(IUserClassificationsReadonly userClassificationsReadonly) {
        return new UserClassificationsDto(userClassificationsReadonly);
    }
}