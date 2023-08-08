using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler;

public class GetAvatarByHashHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<GetAvatarWithHash>, ImageWrapper> {
    public async ValueTask<Result<ImageWrapper, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerGetWrapper<GetAvatarWithHash> request) {
        var db = dbWrapper.Db;
        var controller = request.Controller;
        var prop = request.Get;

        var imageResult = await QueryUserAvatar.GetByHashAsync(db, prop.Hash);

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