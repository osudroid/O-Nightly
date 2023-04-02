using Dapper;
using Npgsql;
using NPoco;

namespace OsuDroid.Database.TableFn; 

public static class BblUserGroupPrivilege {
    public static Result<Entities.BblUserGroupPrivilege[], string> FindGroupPrivilegeByUserId(NpgsqlConnection db, long userId) {
        try {
            var data = db.Query<Entities.BblUserGroupPrivilege>(
                $"SELECT * FROM bbl_user_group_privilege WHERE user_id = {userId}").ToArray();
               
            return Result<Entities.BblUserGroupPrivilege[], string>.Ok(data);
        }
        catch (Exception e) {
            return Result<Entities.BblUserGroupPrivilege[], string>.Err(e.ToString());
        }
    }

    public static ResultErr<string> AddGroupPrivilegeToUserByUserId(NpgsqlConnection db, long userId,
        Guid groupPrivilegeId) {
        try {
            db.Execute(
                "INSERT INTO bbl_user_group_privilege (user_id, group_privilege_id) VALUES (@UserId, @GroupPrivilegeId)",
                new Entities.BblUserGroupPrivilege() { GroupPrivilegeId = groupPrivilegeId, UserId = userId }
            );
            
            return ResultErr<string>.Ok();
        }
        catch (Exception e) {
            return ResultErr<string>.Err(e.ToString());
        }
    }

    public static ResultErr<string> RemoveGroupPrivilegeToUserByUserId(
        NpgsqlConnection db, long userId, Guid groupPrivilegeId) {
        try {
            db.Execute(
                $"DELETE FROM bbl_user_group_privilege WHERE user_id = {userId} AND group_privilege_id = '{groupPrivilegeId}'");
            
            return ResultErr<string>.Ok();
        }
        catch (Exception e) {
            return ResultErr<string>.Err(e.ToString());
        }
    }
}