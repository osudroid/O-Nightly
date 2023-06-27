using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class UserAvatarManager {
    public static async Task<Result<IEnumerable<UserAvatar>, string>> ManyUserIdAndHashAsync(
        NpgsqlConnection db, int pixelSize, IReadOnlyList<long> userIds) {
        return await QueryUserAvatar.GetManyUserIdAndHashByPixelSizeUserIdAsync(db, pixelSize, userIds);
    }
}