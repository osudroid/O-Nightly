using ImageMagick.Formats;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Avatar.Domain.ImageConverter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Avatar.Domain;

/// <summary>
/// Provides methods to manage user avatars, including insertion, retrieval, and conversion operations.
/// </summary>
public sealed class UserAvatarContext: IUserAvatarContext {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly long _userId;
    private readonly IQueryView_UserAvatarNoBytes _queryView_UserAvatarNoBytes;
    private readonly IQueryUserAvatar _queryUserAvatar;
    private readonly IEnvDb _envDb;
    private readonly IImageTo<WebPWriteDefines> _imageToWebP;
    private readonly IImageTo<PngWriteDefines> _imageToPng;

    public UserAvatarContext(
        long userId, 
        IQueryView_UserAvatarNoBytes queryViewUserAvatarNoBytes, 
        IQueryUserAvatar queryUserAvatar, 
        IEnvDb envDb,
        IImageTo<WebPWriteDefines> imageToWebP,
        IImageTo<PngWriteDefines> imageToPng) 
    {
        _userId = userId;
        _queryView_UserAvatarNoBytes = queryViewUserAvatarNoBytes;
        _queryUserAvatar = queryUserAvatar;
        _envDb = envDb;
        _imageToWebP = imageToWebP;
        _imageToPng = imageToPng;
    }

    /// <summary>
    /// Inserts or overwrites the user's avatar with a new one, creating multiple resized versions.
    /// </summary>
    /// <param name="imageBytes">The binary data of the new avatar image.</param>
    /// <returns>Result contains an array of avatar metadata without binary data.</returns>
    public async Task<ResultOk<View_UserAvatarNoBytes[]>> InsertOrOverwriteWithNewAvatarAsync(byte[] imageBytes) {
        var err = await _queryUserAvatar.DeleteAllFromUserIdAsync(this._userId);
        if (err == EResult.Err) {
            return ResultOk<View_UserAvatarNoBytes[]>.Err();
        }
        
        var imageConverterHandler = new ImageConverterHandler(imageBytes, this._imageToWebP);
        var originalResult = imageConverterHandler.CreateOriginalImageDtoAsync()
                                                  .Map(x => x.ToUserAvatar(_userId, true));
        var highResult = imageConverterHandler.CreateHighImageDto()
                                              .Map(x => x.ToUserAvatar(_userId, true));
        var lowResult = imageConverterHandler.CreateLowImageDto()
                                             .Map(x => x.ToUserAvatar(_userId, true));
        
        if (originalResult == EResult.Err || highResult == EResult.Err || lowResult == EResult.Err) {
            return ResultOk<View_UserAvatarNoBytes[]>.Err();
        }
        
        
        if (await _queryUserAvatar.InsertAsync(originalResult.Ok()) == EResult.Err) {
            return ResultOk<View_UserAvatarNoBytes[]>.Err();
        }

        if (await _queryUserAvatar.InsertAsync(highResult.Ok()) == EResult.Err) {
            return ResultOk<View_UserAvatarNoBytes[]>.Err();
        }
        
        if (await _queryUserAvatar.InsertAsync(lowResult.Ok()) == EResult.Err) {
            return ResultOk<View_UserAvatarNoBytes[]>.Err();
        }
        
        return ResultOk<View_UserAvatarNoBytes[]>.Ok([
            View_UserAvatarNoBytes.FromUserAvatar(originalResult.Ok()),
            View_UserAvatarNoBytes.FromUserAvatar(lowResult.Ok()),
            View_UserAvatarNoBytes.FromUserAvatar(highResult.Ok())
        ]);
    }

    /// <summary>
    /// Retrieves all avatars for the user without binary data.
    /// </summary>
    /// <returns>Result contains an array of avatar metadata without binary data.</returns>
    public async Task<ResultOk<View_UserAvatarNoBytes[]>> FindByUserIdAsync() {
        return await _queryView_UserAvatarNoBytes.FindByUserIdAsync(this._userId);
    }

    /// <summary>
    /// Retrieves avatar metadata without binary data for the user by hash.
    /// </summary>
    /// <param name="hash">The hash of the avatar to retrieve.</param>
    /// <returns>Result contains an optional avatar metadata object.</returns>
    public async Task<ResultOk<Option<View_UserAvatarNoBytes>>> FindByUserIdAndHashAsync(string hash) {
        return await _queryView_UserAvatarNoBytes.FindByUserIdAndHashAsync(this._userId, hash);
    }

    /// <summary>
    /// Retrieves the full avatar data for the user by hash.
    /// </summary>
    /// <param name="hash">The hash of the avatar to retrieve.</param>
    /// <returns>Result contains an optional avatar object.</returns>
    public async Task<ResultOk<Option<UserAvatar>>> FindAvatarByUserIdAndHashAsync(string hash) {
        return await _queryUserAvatar.GetByUserIdAndHash(this._userId, hash);
    }

    /// <summary>
    /// Retrieves the low-resolution avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional low-resolution avatar object.</returns>
    public async Task<ResultOk<Option<UserAvatar>>> FindAvatarLowByUserIdAsync() {
        return await _queryUserAvatar.GetLowByUserIdAsync(_userId);
    }

    /// <summary>
    /// Retrieves the high-resolution avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional high-resolution avatar object.</returns>
    public async Task<ResultOk<Option<UserAvatar>>> FindAvatarHighByUserIdAsync() {
        return await _queryUserAvatar.GetHighByUserIdAsync(_userId);
    }

    /// <summary>
    /// Retrieves the original avatar for the user.
    /// </summary>
    /// <returns>Result contains an optional original avatar object.</returns>
    public async Task<ResultOk<Option<UserAvatar>>> FindAvatarOriginalByUserIdAsync() {
        return await _queryUserAvatar.GetOriginalByUserIdAsync(_userId);
    }

    /// <summary>
    /// Converts the given avatar to PNG format.
    /// </summary>
    /// <param name="userAvatar">The avatar to convert.</param>
    /// <returns>Result contains the converted avatar object.</returns>
    public async Task<ResultOk<UserAvatar>> ToPngAsync(UserAvatar userAvatar) {
        if (userAvatar.Bytes is null) {
            Logger.Error("UserAvatarContext.ToPng: UserAvatar.Bytes is null");
            return ResultOk<UserAvatar>.Err();
        }
        try {
            var imageConverterHandler = new ImageConverterHandler(userAvatar.Bytes, this._imageToPng);
            var imagePng = (this._envDb.UserAvatar_ByteSizeLow < userAvatar.PixelSize) switch {
                true => imageConverterHandler.CreateHighImageDto(),
                _ => imageConverterHandler.CreateLowImageDto()
            };

            return imagePng.Map(x => x.ToUserAvatar(userAvatar.UserId, false));
        }
        catch (Exception e) {
            return SResult<UserAvatar>.Err(e).LogIfError(Logger);
        }
    }
}