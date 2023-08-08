using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;

namespace OsuDroid.Handler; 

public class GetAvatarHashesByUserIdsHandler 
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<AvatarHashesByUserIdsDto>, OptionHandlerOutput<ViewAvatarHashes>>{
    public async ValueTask<Result<OptionHandlerOutput<ViewAvatarHashes>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerPostWrapper<AvatarHashesByUserIdsDto> request) {
        
        var db = dbWrapper.Db;
        var controller = request.Controller;
        var prop = request.Post;
        
        var size = (Setting.UserAvatar_SizeLow!.Value >= prop.Size)
            ? Setting.UserAvatar_SizeLow!.Value
            : Setting.UserAvatar_SizeHigh!.Value;
        var resp = await UserAvatarManager.ManyUserIdAndHashAsync(db, prop.Size, prop.UserIds);

        if (resp == EResult.Err)
            return resp.ChangeOkType<OptionHandlerOutput<ViewAvatarHashes>>();

        return Result<OptionHandlerOutput<ViewAvatarHashes>, string>
            .Ok(OptionHandlerOutput<ViewAvatarHashes>.With(new ViewAvatarHashes() {
                List = resp.Ok().Select(x => new ViewAvatarHash { Hash = x.Hash, UserId = x.UserId }).ToList() 
            }));
    }
}