using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Avatar.Adapter.Interface;

public interface IUserAvatarContext {
    /// <summary>
    /// Inserts or overwrites the user's avatar with a new one, creating multiple resized versions.
    /// </summary>
    /// <param name="imageBytes">The binary data of the new avatar image.</param>
    /// <returns>Result contains an array of avatar metadata without binary data.</returns>
    public Task<ResultOk<View_UserAvatarNoBytes[]>> InsertOrOverwriteWithNewAvatarAsync(byte[] imageBytes);

    /// <summary>
    /// Retrieves all avatars for the user without binary data.
    /// </summary>
    /// <returns>Result contains an array of avatar metadata without binary data.</returns>
    public Task<ResultOk<View_UserAvatarNoBytes[]>> FindByUserIdAsync();

    /// <summary>
    /// Retrieves avatar metadata without binary data for the user by hash.
    /// </summary>
    /// <param name="hash">The hash of the avatar to retrieve.</param>
    /// <returns>Result contains an optional avatar metadata object.</returns>
    public Task<ResultOk<Option<View_UserAvatarNoBytes>>> FindByUserIdAndHashAsync(string hash);

    /// <summary>
    /// Retrieves the full avatar data for the user by hash.
    /// </summary>
    /// <param name="hash">The hash of the avatar to retrieve.</param>
    /// <returns>Result contains an optional avatar object.</returns>
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarByUserIdAndHashAsync(string hash);

    /// <summary>
    /// Retrieves the low-resolution avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional low-resolution avatar object.</returns>
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarLowByUserIdAsync();

    /// <summary>
    /// Retrieves the high-resolution avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional high-resolution avatar object.</returns>
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarHighByUserIdAsync();

    /// <summary>
    /// Retrieves the original avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional original avatar object.</returns>
    public Task<ResultOk<Option<UserAvatar>>> FindAvatarOriginalByUserIdAsync();

    /// <summary>
    /// Converts the given avatar to PNG format.
    /// </summary>
    /// <param name="userAvatar">The avatar to convert.</param>
    /// <returns>Result contains the converted avatar object.</returns>
    public Task<ResultOk<UserAvatar>> ToPngAsync(UserAvatar userAvatar);
}