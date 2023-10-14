using Dapper;
using Npgsql;
using OsuDroidLib.Dto;
using OsuDroidLib.Extension;

namespace OsuDroid.Lib;

public static class PrivilegeManager {
    private static readonly object _lock = new();
    private static readonly DateTime LastUpdate = DateTime.UtcNow;
    private static readonly bool First = true;


    public static IReadOnlyDictionary<string, PrivilegeDto> AllPrivilegeByName { get; private set; } =
        new Dictionary<string, PrivilegeDto>();

    public static IReadOnlyDictionary<Guid, PrivilegeDto> AllPrivilegeById { get; private set; }
        = new Dictionary<Guid, PrivilegeDto>();

    public static IReadOnlyDictionary<Guid, PrivilegeDto> GetAllAllowedPrivilegeFromGroupPrivilegeDto(
        UserGroupPrivilegeDto userGroupPrivilegeDto) {
        var mapAllow = new Dictionary<Guid, PrivilegeDto>(8);
        var mapDisallow = new Dictionary<Guid, PrivilegeDto>(8);

        foreach (var (key, value) in userGroupPrivilegeDto.GroupPrivileges)
        foreach (var (guid, (allow, privilegeDto)) in value.Privileges) {
            if (allow) {
                mapAllow[guid] = privilegeDto.CloneType();
                continue;
            }

            mapDisallow[guid] = privilegeDto.CloneType();
        }

        foreach (var (key, value) in mapDisallow) {
            if (!mapAllow.ContainsKey(key))
                continue;
            mapAllow.Remove(key);
        }

        return mapAllow;
    }

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
            UpdateAsync();
            return;
        }

        if (LastUpdate.AddMinutes(10) < DateTime.UtcNow) UpdateAsync();
    }

    public static async Task<ResultErr<string>> UpdateAsync() {
        if (Monitor.IsEntered(_lock)) return ResultErr<string>.Ok();

        var lockTaken = false;
        Monitor.Enter(_lock, ref lockTaken);
        if (lockTaken)
            return ResultErr<string>.Ok();

        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            var result = (await db.SafeQueryAsync<Entities.Privilege>("SELECT * FROM privilege"))
                .Map(x =>
                    x.Select(x => new PrivilegeDto {
                             Id = x.PrivilegeId,
                             Name = x.Name,
                             Description = x.Description
                         }
                     )
                     .ToList()
                );

            if (result == EResult.Err)
                return result;

            var res = result.Ok();

            AllPrivilegeByName = res.ToDictionary(x => x.Name ?? "");
            AllPrivilegeById = res.ToDictionary(x => x.Id);
        }

        Monitor.Exit(lockTaken);
        return ResultErr<string>.Ok();
    }

    public static async Task<Result<List<(string Name, Guid id, bool Has)>, string>> UserCanUse(
        NpgsqlConnection db,
        string needPrivilege,
        long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = await db.QueryAsync<dynamic>(@$"
select * FROM user_check_need_privilege_by_name({userId}, @needPrivilege) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilege }
            );

            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }

    public static async Task<Result<List<(string Name, Guid id, bool Has)>, string>> UserCanUseByIdAsync(
        NpgsqlConnection db,
        Guid needPrivilegeId,
        long userId) {
        try {
            var res = new List<(string Name, Guid id, bool Has)>();
            var query = await db.QueryAsync<dynamic>(@$"
select * FROM user_check_need_privilege_by_id({userId}, @needPrivilegeId) as x(pri_name TEXT, need_privilege_id uuid, user_has_privilege bool)",
                new { needPrivilegeId }
            );

            return Result<List<(string Name, Guid id, bool Has)>, string>.Ok(res);
        }
        catch (Exception e) {
            return Result<List<(string Name, Guid id, bool Has)>, string>.Err(e.ToString());
        }
    }
}