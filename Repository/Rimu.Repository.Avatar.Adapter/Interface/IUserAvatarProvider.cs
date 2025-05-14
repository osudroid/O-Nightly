using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Avatar.Adapter.Interface;

public interface IUserAvatarProvider {
    public IUserAvatarContext CreateNewContext(long userId);
}