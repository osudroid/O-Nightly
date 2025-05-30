using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Avatar.Adapter.Interface;

/// <summary>
/// Provides functionality to create user avatar contexts for managing user avatars.
/// </summary>
public interface IUserAvatarProvider {
    /// <summary>
    /// Creates a new user avatar context for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the context is created.</param>
    /// <returns>An instance of <see cref="IUserAvatarContext"/> for managing the user's avatars.</returns>
    public IUserAvatarContext CreateNewContext(long userId);
}