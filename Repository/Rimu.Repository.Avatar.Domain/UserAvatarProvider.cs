using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Avatar.Domain;

public class UserAvatarProvider: IUserAvatarProvider {
    private readonly IQueryView_UserAvatarNoBytes _queryView_UserAvatarNoBytes;
    private readonly IQueryUserAvatar _queryUserAvatar;
    private readonly IEnvDb _envDb;

    public UserAvatarProvider(IQueryView_UserAvatarNoBytes queryViewUserAvatarNoBytes, IQueryUserAvatar queryUserAvatar, IEnvDb envDb) {
        _queryView_UserAvatarNoBytes = queryViewUserAvatarNoBytes;
        _queryUserAvatar = queryUserAvatar;
        _envDb = envDb;
    }

    public IUserAvatarContext CreateNewContext(long userId) {
        return new UserAvatarContext(
            userId: userId,
            queryViewUserAvatarNoBytes: _queryView_UserAvatarNoBytes,
            queryUserAvatar: _queryUserAvatar,
            envDb: _envDb
        );
    }
}