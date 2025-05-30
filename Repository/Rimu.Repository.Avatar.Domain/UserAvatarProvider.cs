using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Avatar.Domain;

/// <summary>
/// Provides functionality to create user avatar contexts for managing user avatars.
/// </summary>
public class UserAvatarProvider: IUserAvatarProvider {
    private readonly IQueryView_UserAvatarNoBytes _queryView_UserAvatarNoBytes;
    private readonly IQueryUserAvatar _queryUserAvatar;
    private readonly IEnvDb _envDb;

    public UserAvatarProvider(IQueryView_UserAvatarNoBytes queryViewUserAvatarNoBytes, IQueryUserAvatar queryUserAvatar, IEnvDb envDb) {
        _queryView_UserAvatarNoBytes = queryViewUserAvatarNoBytes;
        _queryUserAvatar = queryUserAvatar;
        _envDb = envDb;
    }

    /// <summary>
    /// Creates a new user avatar context for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the context is created.</param>
    /// <returns>An instance of <see cref="IUserAvatarContext"/> for managing the user's avatars.</returns>
    public IUserAvatarContext CreateNewContext(long userId) {
        return new UserAvatarContext(
            userId: userId,
            queryViewUserAvatarNoBytes: _queryView_UserAvatarNoBytes,
            queryUserAvatar: _queryUserAvatar,
            envDb: _envDb
        );
    }
}