using Dapper;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Dto;

namespace OsuDroidLib.Query;

public static class QueryUserGroupPrivilege {
    public static async Task<Result<UserGroupPrivilegeDto, string>>
        GetGroupPrivilegesAsync(NpgsqlConnection db, long userId) {
        try {
            var s = db.Query(@$"
SELECT p.*, gpp.*, gp.* 
FROM privilege p
         join GroupPrivilege_Privilege gpp on p.PrivilegeId = gpp.PrivilegeId
         join GroupPrivilege gp on gpp.GroupPrivilegeId = gp.GroupPrivilegeId
WHERE gp.GroupPrivilegeId IN (SELECT UserGroupPrivilege.GroupPrivilegeId FROM UserGroupPrivilege WHERE UserId = {userId});
"
                      )
                      .ToArray();

            var userDto = new UserGroupPrivilegeDto {
                UserId = userId,
                GroupPrivileges = new Dictionary<Guid, GroupPrivilegeDto>(2)
            };

            foreach (var rowD in s) {
                var row = ((IDictionary<string, object>)rowD).Values.ToList();
                var privilege = new Privilege {
                    PrivilegeId = (Guid)row[0],
                    Name = (string)row[1],
                    Description = (string)row[2]
                };

                var groupPrivilegePrivilege = new GroupPrivilegePrivilege {
                    GroupPrivilegeId = (Guid)row[3],
                    ModeAllow = (bool)row[4],
                    PrivilegeId = (Guid)row[5]
                };

                var groupPrivilege = new GroupPrivilege {
                    GroupPrivilegeId = (Guid)row[6],
                    Name = (string)row[7],
                    Description = (string)row[8]
                };

                GroupPrivilegeDto? groupPrivilegesDto;
                if (userDto.GroupPrivileges.TryGetValue(groupPrivilege.GroupPrivilegeId, out groupPrivilegesDto)
                    == false) {
                    groupPrivilegesDto = new GroupPrivilegeDto {
                        Id = groupPrivilege.GroupPrivilegeId,
                        Description = groupPrivilege.Description,
                        Name = groupPrivilege.Name,
                        Privileges = new Dictionary<Guid, (bool ModeAllow, PrivilegeDto Privilege)>(2)
                    };

                    userDto.GroupPrivileges[groupPrivilege.GroupPrivilegeId] = groupPrivilegesDto;
                }

                (bool ModeAllow, PrivilegeDto Privilege) privilegeDto = default;
                if (groupPrivilegesDto.Privileges.TryGetValue(privilege.PrivilegeId, out privilegeDto) == false) {
                    privilegeDto = (groupPrivilegePrivilege.ModeAllow, new PrivilegeDto {
                        Id = privilege.PrivilegeId,
                        Description = privilege.Description,
                        Name = privilege.Name
                    });
                    groupPrivilegesDto.Privileges[privilege.PrivilegeId] = privilegeDto;
                }
            }

            return Result<UserGroupPrivilegeDto, string>.Ok(userDto);
        }
        catch (Exception e) {
            return Result<UserGroupPrivilegeDto, string>.Err(e.ToString());
        }
    }
}