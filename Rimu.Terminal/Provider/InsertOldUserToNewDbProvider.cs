using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using CsvHelper;
using Dapper;
using LamLibAllOver;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Postgres.Domain.Query;
using Rimu.Terminal.ImportCSVFile;
using Rimu.Terminal.TypeConverter;

namespace Rimu.Terminal.Provider;

internal class InsertOldUserToNewDbProvider {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IQueryUserClassifications _queryUserClassifications;
    private readonly string _pathCSVBblUser;

    internal InsertOldUserToNewDbProvider(
        IQueryUserInfo queryUserInfo,
        IQueryUserStats queryUserStats,
        IQueryUserClassifications queryUserClassifications,
        string pathCSVBblUser) {

        _queryUserInfo = queryUserInfo;
        _queryUserStats = queryUserStats;
        _pathCSVBblUser = pathCSVBblUser;
        _queryUserClassifications = queryUserClassifications;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public async Task RunAsync() {
        Logger.Debug("Fetch Old Users; Filter; Convert");
        var allUserFromOldDb = await GetAllBllUserAsUserInfoAndUserClassificationsAsync();
        Logger.Debug("Insert Users");
        (await InsertAllUserInfoAndUserClassificationsAsync(allUserFromOldDb))
            .MapErr(_ => throw new Exception("Insert UserInfos failed!"));
        (await InsertDefaultAllUserStatsAsync(allUserFromOldDb))
            .ThrowIfErr("Insert UserStats failed!");
    }

    private async Task<UserInfoAndUserClassifications[]> GetAllBllUserAsUserInfoAndUserClassificationsAsync() {
        Logger.Debug("Get All OLD Users");
        
        IList<bbl_user> oldUser = Array.Empty<bbl_user>();
        
        await using (var stream = new FileStream(this._pathCSVBblUser, FileMode.Open, FileAccess.Read, FileShare.None, 4096 * 16, FileOptions.SequentialScan))
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            csv.Context.TypeConverterCache.AddConverter<string>(new NullStringType());
            csv.Context.TypeConverterCache.AddConverter<long?>(new NullLongType());
            csv.Context.TypeConverterCache.AddConverter<double?>(new NullDecimalType());
            csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add("NULL");
            csv.Context.TypeConverterOptionsCache.GetOptions<long?>().NullValues.Add("NULL");
            csv.Context.TypeConverterOptionsCache.GetOptions<double?>().NullValues.Add("NULL");
            
            oldUser = csv
                          .GetRecords<bbl_user>()
                          .ToList()
                ;
        }
        
        Logger.Debug("Fetch Old Users Count: {}", oldUser.Count);
        Logger.Debug("Fix Strings");
        
        
        for (var i = oldUser.Count - 1; i >= 0; i--) {
            var singleUser = oldUser[i];

            if (IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.email)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.password)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.username)
                || singleUser.id < 0) {
                oldUser.RemoveAt(i);
                continue;
            }

            singleUser.password = singleUser.password!.Trim();
            singleUser.username = singleUser.username!.Trim();
            singleUser.email = singleUser.email!.Trim();
        }

        Logger.Debug("Convert To UserInfo");
        var userInfoAndUserClassificationsList = new List<UserInfoAndUserClassifications>(oldUser.Count);
        foreach (var bblUser in oldUser) {
            var userInfo = new UserInfo() {
                Active = bblUser.active == 1,
                Banned = bblUser.banned == 1,
                DeviceId = "",
                Email = bblUser.email ?? throw new NullReferenceException(nameof(bblUser.email)),
                UserId = bblUser.id,
                Password = bblUser.password ?? throw new NullReferenceException(nameof(bblUser.password)),
                PasswordGen2 = "",
                Region = (bblUser.region ?? "").ToUpper(),
                Username = bblUser.username ?? throw new NullReferenceException(nameof(bblUser.username)),
                PatronEmail = null,
                LatestIp = bblUser.regist_ip ?? "",
                RegisterTime = DateTime.SpecifyKind(bblUser.regist_time, DateTimeKind.Utc),
                RestrictMode = false,
                LastLoginTime = DateTime.SpecifyKind(bblUser.last_login_time, DateTimeKind.Utc),
                PatronEmailAccept = false,
                UsernameLastChange = DateTime.SpecifyKind(bblUser.regist_time, DateTimeKind.Utc),
                Archived = bblUser.archived != 0,
            };

            var userClassifications = new UserClassifications() {
                UserId = bblUser.id,
                Contributor = bblUser.contributor == 1,
                Developer = bblUser.developer == 1,
                Supporter = bblUser.supporter == 1,
                CoreDeveloper = bblUser.contributor == 1
            };

            if (userInfo.Email is null || userInfo.Email.Length == 0) continue;
            if (userInfo.Password is null || userInfo.Password.Length == 0) continue;
            if (userInfo.Region is null || userInfo.Region.Length == 0) {
                userInfo.Region = "";
            }

            if (userInfo.Username is null || userInfo.Username.Length == 0) continue;

            userInfoAndUserClassificationsList.Add(new () {
                UserClassifications = userClassifications,
                UserInfo = userInfo,
            });
        }

        Logger.Debug("Remove Equals");
        return RemoveAllEqUsername(RemoveAllEqEmails(userInfoAndUserClassificationsList.ToArray()));
    }

    private UserInfoAndUserClassifications[] RemoveAllEqUsername(UserInfoAndUserClassifications[] userInfoAndUserClassificationsArr) {
        Array.Sort(userInfoAndUserClassificationsArr, new UserInfoAndUserClassificationsComparerUserId());

        var dictionary = new Dictionary<string, UserInfoAndUserClassifications>(128000);
        foreach (var userInfoAndUserClassifications in userInfoAndUserClassificationsArr) {
            if (dictionary.ContainsKey(userInfoAndUserClassifications.UserInfo.Username!))
                continue;
            dictionary[userInfoAndUserClassifications.UserInfo.Email!] = userInfoAndUserClassifications;
        }

        var res = new UserInfoAndUserClassifications[dictionary.Count];
        var posi = 0;
        foreach (var (key, value) in dictionary) {
            res[posi] = value;
            posi++;
        }

        Array.Sort(res, new UserInfoAndUserClassificationsComparerUserId());
        return res;
    }

    private UserInfoAndUserClassifications[] RemoveAllEqEmails(UserInfoAndUserClassifications[] userInfoAndUserClassificationsArr) {
        Array.Sort(userInfoAndUserClassificationsArr, new UserInfoAndUserClassificationsComparerUserId());
        var dictionary = new Dictionary<string, UserInfoAndUserClassifications>(userInfoAndUserClassificationsArr.Length * 2);
        foreach (var userInfoAndUserClassifications in userInfoAndUserClassificationsArr) {
            if (dictionary.ContainsKey(userInfoAndUserClassifications.UserInfo.Email!))
                continue;
            dictionary[userInfoAndUserClassifications.UserInfo.Email!] = userInfoAndUserClassifications;
        }

        var res = new UserInfoAndUserClassifications[dictionary.Count];
        var posi = 0;
        foreach (var (key, value) in dictionary) {
            res[posi] = value;
            posi++;
        }

        return res;
    }

    private bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value) {
        return value is null || value.Trim() is "" or " " or "NULL" or "null";
    }

    private async Task<SResultErr> InsertAllUserInfoAndUserClassificationsAsync(UserInfoAndUserClassifications[] userInfoAndUserClassificationsArr) {
        int countChunk = 0;
        foreach (var userInfoChunk in userInfoAndUserClassificationsArr.Select(x => x.UserInfo).Chunk(10_000)) {
            Logger.Debug("Inser UserInfo Count Chunk: " + countChunk);
            
            (await _queryUserInfo.InsertBulkAsync(userInfoChunk)).ThrowIfErr("Try Insert UserInfos");
            
            countChunk++;
        }
        
        foreach (var userClassificationsChunk in userInfoAndUserClassificationsArr.Select(x => x.UserClassifications).Chunk(10_000)) {
            Logger.Debug("Inser UserClassifications Count Chunk: " + countChunk);
            
            (await _queryUserClassifications.InsertBulkAsync(userClassificationsChunk)).ThrowIfErr("Try Insert UserInfos");
            
            countChunk++;
        }
        
        return SResultErr.Ok();
    }
    
    private async Task<ResultNone> InsertDefaultAllUserStatsAsync(UserInfoAndUserClassifications[] userInfoAndUserClassificationsArr) {
        foreach (var userStatsChunk in userInfoAndUserClassificationsArr.Select<UserInfoAndUserClassifications, UserStats>(x => new UserStats() {
                                   UserId = x.UserInfo.UserId,
                                   OverallPlaycount = 0,
                                   OverallScore = 0,
                                   OverallAccuracy = 0,
                                   OverallCombo = 0,
                                   OverallXss = 0,
                                   OverallSs = 0,
                                   OverallXs = 0,
                                   OverallS = 0,
                                   OverallA = 0,
                                   OverallB = 0,
                                   OverallC = 0,
                                   OverallD = 0,
                                   OverallPerfect = 0,
                                   OverallHits = 0,
                                   Overall300 = 0,
                                   Overall100 = 0,
                                   Overall50 = 0,
                                   OverallGeki = 0,
                                   OverallKatu = 0,
                                   OverallMiss = 0,
                                   OverallPp = 0,
                               })
                               .Chunk(10_000)) {
            return await _queryUserStats.InsertOrUpdateBulkAsync(userStatsChunk);
        }

        return ResultNone.Ok;
    }
}

internal class UserInfoComparerEmail : IComparer<UserInfo> {
    public int Compare(UserInfo? left, UserInfo? right) {
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;

        var x = left.Email;
        var y = right.Email;
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}

internal class UserInfoComparerUser : IComparer<UserInfo> {
    public int Compare(UserInfo? left, UserInfo? right) {
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;

        var x = left.Username;
        var y = right.Username;
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}

internal class UserInfoComparerUserId : IComparer<UserInfo> {
    public int Compare(UserInfo? left, UserInfo? right) {
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

internal class UserInfoAndUserClassificationsComparerUserId : IComparer<UserInfoAndUserClassifications> {
    public int Compare(UserInfoAndUserClassifications left, UserInfoAndUserClassifications right) {
        if (left == default && right == default)
            return 0;
        if (left == default)
            return -1;
        if (right == default)
            return 1;
        
        var x = left.UserInfo.UserId;
        var y = right.UserInfo.UserId;
        return x.CompareTo(y);
    }
}

public struct UserInfoAndUserClassifications : IEquatable<UserInfoAndUserClassifications> {
    public required UserInfo UserInfo { get; init; }
    public required UserClassifications UserClassifications { get; init; }
    
    public static bool operator ==(UserInfoAndUserClassifications a, UserInfoAndUserClassifications b) {
        if (a.UserInfo is null && b.UserInfo is null) {
            return true;
        } 
        
        if (a.UserInfo is null || b.UserInfo is null) {
            return false;
        } 
        
        return a.UserInfo.UserId == b.UserInfo.UserId;
    }

    public static bool operator !=(UserInfoAndUserClassifications a, UserInfoAndUserClassifications b) {
        return a.UserInfo.UserId != b.UserInfo.UserId;
    }
    
    public static bool operator >(UserInfoAndUserClassifications a, UserInfoAndUserClassifications b) {
        return a.UserInfo.UserId > b.UserInfo.UserId;
    }

    public static bool operator <(UserInfoAndUserClassifications a, UserInfoAndUserClassifications b) {
        return a.UserInfo.UserId < b.UserInfo.UserId;
    }
    
    public bool Equals(UserInfoAndUserClassifications other) {
        return other == this;
    }

    public override bool Equals(object? obj) {
        return obj is UserInfoAndUserClassifications other && other == this;
    }

    public override int GetHashCode() {
        return UserInfo.UserId.GetHashCode();
    }
}