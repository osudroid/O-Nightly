using Dapper;
using Npgsql;
using OsuDroidLib.Dto;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib; 

public static class PrivilegeManager {
    public static Result<BblUserGroupPrivilegeDto, string> GetGroupPrivileges(NpgsqlConnection db, long userId) {
        try {
            var s = db.Query(@$"
SELECT p.*, gpp.*, gp.* 
FROM privilege p
         join group_privilege_privilege gpp on p.id = gpp.privilege_id
         join group_privilege gp on gpp.group_privilege_id = gp.id
WHERE gp.id IN (SELECT bbl_user_group_privilege.group_privilege_id FROM bbl_user_group_privilege WHERE user_id = {userId});
").ToArray();
        
            BblUserGroupPrivilegeDto userDto = new BblUserGroupPrivilegeDto() {
                UserId = 22578,
                GroupPrivileges = new (2)
            };
            
            foreach (var rowD in s) {
                var row = ((IDictionary<string, Object>)rowD).Values.ToList();
                var privilege = new Privilege() {
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
            
                GroupPrivilegeDto? groupPrivilegesDto;
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
            
            return Result<BblUserGroupPrivilegeDto, string>.Ok(userDto);
        }
        catch (Exception e) {
            return Result<BblUserGroupPrivilegeDto, string>.Err(e.ToString());
        }
    }

    public static IReadOnlyDictionary<Guid, PrivilegeDto> GetAllAllowedPrivilegeFromGroupPrivilegeDto(BblUserGroupPrivilegeDto bblUserGroupPrivilegeDto) {
        var mapAllow = new Dictionary<Guid, PrivilegeDto>(8);
        var mapDisallow = new Dictionary<Guid, PrivilegeDto>(8);

        foreach (var (key, value) in bblUserGroupPrivilegeDto.GroupPrivileges) {
            foreach (var (guid, (allow, privilegeDto)) in value.Privileges) {
                if (allow) {
                    mapAllow[guid] = privilegeDto.CloneType();
                    continue;
                }
                mapDisallow[guid] = privilegeDto.CloneType();
            }
        }

        foreach (var (key, value) in mapDisallow) {
            if (!mapAllow.ContainsKey(key)) 
                continue;
            mapAllow.Remove(key);
        }

        return mapAllow;
    }


    public static IReadOnlyDictionary<string, PrivilegeDto> AllPrivilegeByName { get; private set; } =
        new Dictionary<string, PrivilegeDto>();

    public static IReadOnlyDictionary<Guid, PrivilegeDto> AllPrivilegeById { get; private set; } 
        = new Dictionary<Guid, PrivilegeDto>();
    
    
    
    private static object _lock = new Object();
    private static DateTime LastUpdate = DateTime.UtcNow;
    private static bool First = true;
    
    public static Option<PrivilegeDto> UserHasPrivilegeByName(BblUserGroupPrivilegeDto privilegeDto, string name) {
        UpdateIfNeeded();
        AllPrivilegeByName.TryGetValue(name, out var res);
        return Option<PrivilegeDto>.NullSplit(res);
    }
    
    public static Option<PrivilegeDto> UserHasPrivilegeById(BblUserGroupPrivilegeDto privilegeDto, Guid id) {
        UpdateIfNeeded();
        AllPrivilegeById.TryGetValue(id, out var res);
        return Option<PrivilegeDto>.NullSplit(res);
    }
    
    private static void UpdateIfNeeded() {
        if (First) {
            Update();
            return;
        }

        if (LastUpdate.AddMinutes(10) < DateTime.UtcNow) {
            Update();
            return;
        }
    }
    public static void Update() {
        if (Monitor.IsEntered(_lock)) return;
        
        bool lockTaken = false;
        Monitor.Enter(_lock, ref lockTaken);
        if (lockTaken)
            return;

        using (var db = DbBuilder.BuildPostSqlAndOpen()) {
            var res = db.Fetch<Privilege>("SELECT * FROM privilege")
                .OkOr(new())
                .Select(x => new PrivilegeDto { Id = x.Id, Name = x.Name, Description = x.Description}).ToList();
            AllPrivilegeByName = res.ToDictionary(x => x.Name??"");
            AllPrivilegeById = res.ToDictionary(x => x.Id);
        }
        
        Monitor.Exit(lockTaken);
    }

    public static Result<List<(string Name, Guid id, bool Has)>, string> UserCanUse(NpgsqlConnection db, string needPrivilege, long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = db.Query<dynamic>(@$"
select * FROM user_check_need_privilege_by_name({userId}, @needPrivilege) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilege=needPrivilege });
            
            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }
    
    public static Result<List<(string Name, Guid id, bool Has)>, string> UserCanUseById(NpgsqlConnection db, Guid needPrivilegeId, long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = db.Query<dynamic>(@$"
select * FROM user_check_need_privilege_by_id({userId}, @needPrivilegeId) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilegeId=needPrivilegeId });
            
            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }
}