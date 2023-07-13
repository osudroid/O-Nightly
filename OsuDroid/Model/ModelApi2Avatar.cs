using Npgsql;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroid.Extensions;
using OsuDroidLib.Extension;
using OsuDroidLib.Manager;

namespace OsuDroid.Model;

public static class ModelApi2Avatar {
    public static async Task<Result<ModelResult<ViewAvatarHashes>, string>> AvatarHashesByUserIdsAsync(
        ControllerExtensions controller, NpgsqlConnection db, AvatarHashesByUserIdsDto avatarHashesByUserIds) {
        var size = (Setting.UserAvatar_SizeLow!.Value >= avatarHashesByUserIds.Size)
            ? Setting.UserAvatar_SizeLow!.Value
            : Setting.UserAvatar_SizeHigh!.Value;
        var resp = await UserAvatarManager
            .ManyUserIdAndHashAsync(db, avatarHashesByUserIds.Size, avatarHashesByUserIds.UserIds);

        if (resp == EResult.Err)
            resp.ChangeOkType<ModelResult<ViewAvatarHashes>>();

        return Result<ModelResult<ViewAvatarHashes>, string>
            .Ok(ModelResult<ViewAvatarHashes>
                .Ok(new ViewAvatarHashes() {
                    List = resp.Ok().Select(x => new ViewAvatarHash { Hash = x.Hash, UserId = x.UserId }).ToList()
                }));
    }
}