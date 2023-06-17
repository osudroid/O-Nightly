using System.Net.Mime;
using Npgsql;
using OsuDroidLib.Class;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using OsuDroidLib.Query;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace OsuDroidLib.Lib; 

public static class UserAvatarHandler {
    /// <returns> Hash </returns>
    public static async Task<ResultErr<string>> UpdateImageForUserAsync(NpgsqlConnection db, long userId, byte[] bytes) {
        var resultErr = await QueryUserAvatar.DeleteAllFromUserIdAsync(db, userId);
        if (resultErr == EResult.Err)
            return resultErr;

        var originalResult = await CreateOriginalUserAvatarAsync(bytes, userId);
        if (originalResult == EResult.Err)
            return originalResult;
        
        if (originalResult.Ok().IsNotSet())
            return ResultErr<string>.Err(TraceMsg.WithMessage("False Image Type Or Can Not Read Image"));

        var setting = new SettingUserAvatar(Setting.UserAvatar_SizeLow!.Value, Setting.UserAvatar_SizeHigh!.Value);
        
        var original = originalResult.Ok().Unwrap();
        
        await QueryUserAvatar.InsertAsync(db, original.UserAvatar);
        
        await using var imageMemoryStream = new MemoryStream(bytes);
        {
            // Low
            var resultLow = await CreateUserAvatarAsync(
                setting, imageMemoryStream, true, userId, original.ImageFormat, original.UserAvatar);
            if (resultLow == EResult.Err)
                return resultLow;

            await QueryUserAvatar.InsertAsync(db, resultLow.Ok());
        }

        {
            // High
            var resultHigh = await CreateUserAvatarAsync(
                setting, imageMemoryStream, false, userId, original.ImageFormat, original.UserAvatar);
            if (resultHigh == EResult.Err)
                return resultHigh;

            await QueryUserAvatar.InsertAsync(db, resultHigh.Ok());
        }
        
        return ResultErr<string>.Ok();
    }

    private static readonly WebpEncoder WebpEncoderSetting = new() {
        Quality = 80,
        FileFormat = WebpFileFormatType.Lossy,
        FilterStrength = 80,
        NearLossless = false,
        UseAlphaCompression = true,
        TransparentColorMode = WebpTransparentColorMode.Preserve,
        SpatialNoiseShaping = 60,
        EntropyPasses = 2,
        Method = WebpEncodingMethod.Level5
    };
    private static async Task<Result<UserAvatar, string>> CreateUserAvatarAsync(
        SettingUserAvatar setting, MemoryStream mem, bool toLow, long userId, IImageFormat imageFormat, UserAvatar userAvatarOri) {
        try {
            using var image = await Image.LoadAsync(mem);
            
            switch (imageFormat.Name) {
                case "Gif":
                case "gif":
                    return Result<UserAvatar, string>.Ok(userAvatarOri);
                default:
                    break;
            }
            

            image.Mutate(x => {
                if (toLow) {
                    x.Resize(setting.SizeLow, setting.SizeLow);
                    return;
                }

                x.Resize(setting.SizeHigh, setting.SizeHigh);
            });
            
            await using var imageMemoryRes = new MemoryStream();
            
            await image.SaveAsWebpAsync(imageMemoryRes, WebpEncoderSetting);

            var bytes = imageMemoryRes.ToArray();

            await imageMemoryRes.WriteAsync(BitConverter.GetBytes(userId));
            return Result<UserAvatar, string>.Ok(new UserAvatar() {
                Bytes = bytes,
                UserId = userId,
                TypeExt = "webp",
                Hash = Sha3.GetSha3Byte(imageMemoryRes.ToArray()),
                Animation = false,
                PixelSize = image.Size.Width
            });
        }
        catch (Exception e) {
            return Result<UserAvatar, string>.Err(e.ToString());
        }
    }

    private static async Task<Result<Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>, string>> 
        CreateOriginalUserAvatarAsync(byte[] bytes, long userId) {
        
        try {
            ImageInfo imageInfo = Image.Identify(bytes);
            IImageFormat? imageFormat = imageInfo.Metadata.DecodedImageFormat;
            if (imageFormat is null)
                return Result<Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>, string>.Ok(
                    Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>.Empty);
            
            var list = bytes.ToList();
            foreach (var b in BitConverter.GetBytes(userId)) {
                list.Add(b);
            }
            
            var userAvatar = new UserAvatar {
                Bytes = bytes,
                UserId = userId,
                TypeExt = imageFormat.Name,
                Hash = Sha3.GetSha3Byte(list.ToArray()),
                Animation = false,
                PixelSize = imageInfo.Size.Width
            };
            
            return Result<Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>, string>
                .Ok(Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>
                    .With((imageFormat, userAvatar)));
        }
        catch (Exception e) {
            return Result<Option<(IImageFormat ImageFormat, UserAvatar UserAvatar)>, string>.Err(e.ToString());
        }
    }
    
    public static async Task<Result<Option<UserAvatar>, string>> GetByUserIdAsync(NpgsqlConnection db, long userId, bool low) {
        return low
            ? await QueryUserAvatar.GetLowByUserIdAsync(db, userId)
            : await QueryUserAvatar.GetHighByUserIdAsync(db, userId);
    }

    public static async Task<Result<Option<UserAvatar>, string>> GetByHashAsync(NpgsqlConnection db, string hash) {
        return await QueryUserAvatar.GetByHashAsync(db, hash);
    }
}