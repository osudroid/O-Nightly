using ImageMagick.Formats;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Avatar.Domain.ImageConverter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Avatar.Domain;

/// <summary>
/// Provides functionality to create user avatar contexts for managing user avatars.
/// </summary>
public class UserAvatarProvider: IUserAvatarProvider {
    private readonly IQueryView_UserAvatarNoBytes _queryViewUserAvatarNoBytes;
    private readonly IQueryUserAvatar _queryUserAvatar;
    private readonly IEnvDb _envDb;
    private readonly IImageTo<WebPWriteDefines> _imageToWebP;
    private readonly IImageTo<PngWriteDefines> _imageToPng;

    public UserAvatarProvider(
            IQueryView_UserAvatarNoBytes queryViewUserAvatarNoBytes,
            IQueryUserAvatar queryUserAvatar,
            IEnvDb envDb,
            IImageTo<WebPWriteDefines> imageToWebP,
            IImageTo<PngWriteDefines> imageToPng
        ) {
        IQueryView_UserAvatarNoBytes _queryViewUserAvatarNoBytes = queryViewUserAvatarNoBytes;
        IQueryUserAvatar _queryUserAvatar = queryUserAvatar;
        IEnvDb _envDb = envDb;
        IImageTo<WebPWriteDefines> _imageToWebP = imageToWebP;
        IImageTo<PngWriteDefines> _imageToPng = imageToPng;
    }

    /// <summary>
    /// Creates a new user avatar context for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the context is created.</param>
    /// <returns>An instance of <see cref="IUserAvatarContext"/> for managing the user's avatars.</returns>
    public IUserAvatarContext CreateNewContext(long userId) {
        return new UserAvatarContext(
                userId,
                _queryViewUserAvatarNoBytes,
                _queryUserAvatar,
                _envDb,
                _imageToWebP,
                _imageToPng
        );
    }
}