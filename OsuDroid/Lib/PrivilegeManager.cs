using Dapper;
using Npgsql;
using OsuDroidLib.Dto;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib; 

public static class PrivilegeManager {
    public static BblUserGroupPrivilegeDto? GetPrivileges(NpgsqlConnection db, long userId) {
        var s = db.Query(@$"
SELECT p.*, gpp.*, gp.* 
FROM privilege p
         join group_privilege_privilege gpp on p.id = gpp.privilege_id
         join group_privilege gp on gpp.group_privilege_id = gp.id
WHERE gp.id IN (SELECT bbl_user_group_privilege.group_privilege_id FROM bbl_user_group_privilege WHERE user_id = {userId});
").ToArray();
        
            BblUserGroupPrivilegeDto userDto = new OsuDroidLib.Dto.BblUserGroupPrivilegeDto() {
                UserId = 22578,
                GroupPrivileges = new (2)
            };
            
            foreach (var rowD in s) {
                var row = ((IDictionary<string, Object>)rowD).Values.ToList();
                var privilege = new Entities.Privilege() {
                    Id = (Guid)row[0],
                    Name = (string)row[1],
                    Description = (string)row[2]
                };
            
                var groupPrivilegePrivilege = new GroupPrivilegePrivilege() {
                    GroupPrivilegeId = (Guid)row[3],
                    ModeAllow = (bool)row[4],
                    PrivilegeId = (Guid)row[5],
                };
                
                var groupPrivilege = new GroupPrivilege() {
                    Id = (Guid)row[6],
                    Name = (string)row[7], 
                    Description = (string)row[8] 
                };
            
                GroupPrivilegeDto groupPrivilegesDto;
                if (userDto.GroupPrivileges.TryGetValue(groupPrivilege.Id, out groupPrivilegesDto) == false) {
                    groupPrivilegesDto = new GroupPrivilegeDto() {
                        Id = groupPrivilege.Id,
                        Description = groupPrivilege.Description,
                        Name = groupPrivilege.Name,
                        Privileges = new(2)
                    };
            
                    userDto.GroupPrivileges[groupPrivilege.Id] = groupPrivilegesDto;
                }
            
                (bool ModeAllow, PrivilegeDto Privilege) privilegeDto = default;
                if (groupPrivilegesDto.Privileges.TryGetValue(privilege.Id, out privilegeDto) == false) {
                    privilegeDto = (groupPrivilegePrivilege.ModeAllow, new PrivilegeDto() {
                        Id = privilege.Id,
                        Description = privilege.Description,
                        Name = privilege.Name
                    });
                    groupPrivilegesDto.Privileges[privilege.Id] = privilegeDto;
                }
            }
            
            return userDto;
    }
}