using Dapper;
using OsuDroid.Database.OldEntities;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using OsuDroid.Database.OldEntities;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Dapper;
using Dapper.Contrib.Extensions;
using OsuDroidLib.Dto;

namespace OsuDroid.Lib.DbTransfer; 

public static class InsertUserHandler {
    public static async Task Run() {
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            WriteLine($"Fetch Old Users; Filter; Convert");
            var allUserFromOldDb = await GetAllBllUserAsUserInfoAsync();
            WriteLine("Insert Users");
            await InsertAllUsers(db, allUserFromOldDb);
        }
    }

    public static async Task<Entities.UserInfo[]> GetAllBllUserAsUserInfoAsync() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();

        WriteLine($"Fetch Old Users");
        var bblUsersOld = (await db.QueryAsync<bbl_user>(
            $"SELECT * FROM {Setting.OldDatabase}.bbl_user"))
            .ToList();
        WriteLine($"Fetch Old Users Count: " + bblUsersOld.Count);
        
        WriteLine("Fix Strings");
        for (var i = bblUsersOld.Count - 1; i >= 0; i--) {
            var singleUser = bblUsersOld[i];

            if (IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.email)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.password)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.username)
                || singleUser.id < 0) {
                bblUsersOld.RemoveAt(i);
                continue;
            }

            singleUser.password = singleUser.password!.Trim();
            singleUser.username = singleUser.username!.Trim();
            singleUser.email = singleUser.email!.Trim();
        }

        WriteLine("Convert To UserInfo");
        var userInfoList = new List<UserInfo>(bblUsersOld.Count);
        foreach (var bblUser in bblUsersOld) {
            userInfoList.Add(new Entities.UserInfo {
                Active = bblUser.active == 1,
                Banned = bblUser.banned == 1,
                DeviceId = "",
                Email = bblUser.email,
                UserId = bblUser.id,
                Password = bblUser.password,
                Region = (bblUser.region ?? "").ToUpper(),
                Username = bblUser.username,
                PatronEmail = null,
                LatestIp = bblUser.regist_ip ?? "",
                RegisterTime = DateTime.SpecifyKind(bblUser.regist_time, DateTimeKind.Utc),
                RestrictMode = false,
                LastLoginTime = DateTime.SpecifyKind(bblUser.last_login_time, DateTimeKind.Utc),
                PatronEmailAccept = false,
                UsernameLastChange = DateTime.SpecifyKind(bblUser.regist_time, DateTimeKind.Utc)
            });
        }
        
        WriteLine("Remove Equals");
        return RemoveAllEqUsername(RemoveAllEqEmails(userInfoList.ToArray()));
    }
    
    private static Entities.UserInfo[] RemoveAllEqUsername(Entities.UserInfo[] UserInfoArr) {
        Array.Sort(UserInfoArr, new UserInfoComparerUserId());
        
        var dictionary = new Dictionary<string, Entities.UserInfo>(128000);
        foreach (var bblUser in UserInfoArr) {
            if (dictionary.ContainsKey(bblUser.Username!))
                continue;
            dictionary[bblUser.Email!] = bblUser;
        }

        var res = new UserInfo[dictionary.Count];
        var posi = 0;
        foreach (var (key, value) in dictionary) {
            res[posi] = value;
            posi++;
        }
        
        Array.Sort(res, new UserInfoComparerUserId());
        return res;
    }
    
    private static Entities.UserInfo[] RemoveAllEqEmails(Entities.UserInfo[] UserInfoArr) {
        Array.Sort(UserInfoArr, new UserInfoComparerUserId());
        var dictionary = new Dictionary<string, Entities.UserInfo>(UserInfoArr.Length * 2);
        foreach (var bblUser in UserInfoArr) {
            if (dictionary.ContainsKey(bblUser.Email!))
                continue;
            dictionary[bblUser.Email!] = bblUser;
        }

        var res = new UserInfo[dictionary.Count];
        var posi = 0;
        foreach (var (key, value) in dictionary) {
            res[posi] = value;
            posi++;
        }
        
        return res;
    }
    
    private static bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value)
        => value is null || value.Trim() is "" or " " or "NULL" or "null";
    
    private static async Task InsertAllUsers(NpgsqlConnection db, UserInfo[] users) {
        await db.ExecuteAsync(@"
Insert Into public.UserInfo (UserId, Username, Password, Email, DeviceId, RegisterTime, LastLoginTime, LatestIp, region, active, banned, RestrictMode, UsernameLastChange, PatronEmail, PatronEmailAccept)
VALUES (
        @UserId, 
        @Username, 
        @Password, 
        @Email, 
        @DeviceId, 
        @RegisterTime, 
        @LastLoginTime, 
        @LatestIp, 
        @Region, 
        @Active, 
        @Banned, 
        @RestrictMode, 
        @UsernameLastChange, 
        @PatronEmail, 
        @PatronEmailAccept)
", users);
    }
}

public class UserInfoComparerEmail : IComparer<Entities.UserInfo> {
    public int Compare(Entities.UserInfo? left, Entities.UserInfo? right) {
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;
        
        var x = left.Email;
        var y = right.Email;
        return String.Compare(x, y, StringComparison.Ordinal);
    }
}

public class UserInfoComparerUser : IComparer<Entities.UserInfo> {
    public int Compare(Entities.UserInfo? left, Entities.UserInfo? right) {
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;
        
        var x = left.Username;
        var y = right.Username;
        return String.Compare(x, y, StringComparison.Ordinal);
    }
}

public class UserInfoComparerUserId : IComparer<Entities.UserInfo> {
    public int Compare(Entities.UserInfo? left, Entities.UserInfo? right) {
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;
        
        var x = left.UserId;
        var y = right.UserId;
        return x.CompareTo(y);
    }
}