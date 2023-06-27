using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using OsuDroidLib.Dto;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroid.Lib;

public static class PrivilegeManager {
    public static IReadOnlyDictionary<Guid, PrivilegeDto> GetAllAllowedPrivilegeFromGroupPrivilegeDto(
        UserGroupPrivilegeDto userGroupPrivilegeDto) {
        var mapAllow = new Dictionary<Guid, PrivilegeDto>(8);
        var mapDisallow = new Dictionary<Guid, PrivilegeDto>(8);

        foreach (var (key, value) in userGroupPrivilegeDto.GroupPrivileges) {
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

    public static Option<PrivilegeDto> UserHasPrivilegeByName(UserGroupPrivilegeDto privilegeDto, string name) {
        UpdateIfNeeded();
        AllPrivilegeByName.TryGetValue(name, out var res);
        return Option<PrivilegeDto>.NullSplit(res);
    }

    public static Option<PrivilegeDto> UserHasPrivilegeById(UserGroupPrivilegeDto privilegeDto, Guid id) {
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

    public static async Task<ResultErr<string>> Update() {
        if (Monitor.IsEntered(_lock)) return ResultErr<string>.Ok();

        bool lockTaken = false;
        Monitor.Enter(_lock, ref lockTaken);
        if (lockTaken)
            return ResultErr<string>.Ok();

        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            var result = (await db.SafeQueryAsync<Privilege>("SELECT * FROM privilege"))
                .Map(x =>
                    x.Select(x => new PrivilegeDto {
                        Id = x.PrivilegeId,
                        Name = x.Name,
                        Description = x.Description
                    }).ToList());

            if (result == EResult.Err)
                return result;

            var res = result.Ok();

            AllPrivilegeByName = res.ToDictionary(x => x.Name ?? "");
            AllPrivilegeById = res.ToDictionary(x => x.Id);
        }

        Monitor.Exit(lockTaken);
        return ResultErr<string>.Ok();
    }

    public static Result<List<(string Name, Guid id, bool Has)>, string> UserCanUse(NpgsqlConnection db,
        string needPrivilege, long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = db.Query<dynamic>(@$"
select * FROM user_check_need_privilege_by_name({userId}, @needPrivilege) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilege = needPrivilege });

            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }

    public static Result<List<(string Name, Guid id, bool Has)>, string> UserCanUseById(NpgsqlConnection db,
        Guid needPrivilegeId, long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = db.Query<dynamic>(@$"
select * FROM user_check_need_privilege_by_id({userId}, @needPrivilegeId) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilegeId = needPrivilegeId });

            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }
}