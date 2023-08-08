using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class GetAvatarHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<GetAvatar>, ImageWrapper> {
    public async ValueTask<Result<ImageWrapper, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger,
        ControllerGetWrapper<GetAvatar> request) {
        var db = dbWrapper.Db;
        var controller = request.Controller;
        var prop = request.Get;

        var imageResult = Setting.UserAvatar_SizeLow!.Value >= prop.Size
            ? await QueryUserAvatar.GetLowByUserIdAsync(db, prop.Id)
            : await QueryUserAvatar.GetHighByUserIdAsync(db, prop.Id);

        if (imageResult == EResult.Err)
            return Result<ImageWrapper, string>.Err(imageResult.Err());

        var imageOpt = imageResult.Ok();
        if (imageOpt.IsNotSet())
            return Result<ImageWrapper, string>.Ok(new ImageWrapper(default));

        var image = imageOpt.Unwrap();

        return Result<ImageWrapper, string>
            .Ok(new ImageWrapper(Option<(byte[] Bytes, string Ext)>
                .With((image.Bytes!, image.TypeExt!))));
    }
}