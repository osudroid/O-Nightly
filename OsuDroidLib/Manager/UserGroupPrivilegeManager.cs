using Npgsql;
using OsuDroidLib.Dto;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class UserGroupPrivilegeManager {
    public static async Task<Result<UserGroupPrivilegeDto, string>> GetGroupPrivilegesAsync(
        NpgsqlConnection db, 
        long userId) {

        return await QueryUserGroupPrivilege.GetGroupPrivilegesAsync(db, userId);
    }
}