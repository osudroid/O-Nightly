using System.Net;
using NPoco;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;
using MD5 = System.Security.Cryptography.MD5;

namespace OsuDroid.Database.TableFn;

public static class BblUser {
    /// <summary> Use input string to calculate MD5 hash </summary>
    /// <param name="input"> passwd </param>
    /// <returns> MD5 Hash </returns>
    public static string HashPasswd(string passwd, string seed) {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(passwd + seed);
        var hashBytes = md5.ComputeHash(inputBytes);
        var res = Convert.ToHexString(hashBytes); // .NET 5 +
        return res.ToLower();
    }

    public static bool PasswordEqual(Entities.BblUser bblUser, string passwd) {
        return bblUser.Password == LamLibAllOver.MD5.Hash(passwd + Env.PasswdSeed).ToLower();
    }

    public static Entities.BblUser? GetUserById(SavePoco db, long id) {
        return db.FirstOrDefault<Entities.BblUser>($"SELECT * FROM bbl_user WHERE id = {id} LIMIT 1") switch {
            { Status: EResponse.Ok } res =>
                res.Ok() is null ? null : res.Ok(),
            _ => null
        };
    }

    public static DateTime UpdateLastLoginTime(Entities.BblUser bblUser, SavePoco db) {
        return UpdateLastLoginTime(db, bblUser.Id);
    }

    public static DateTime UpdateLastLoginTime(SavePoco db, long userId) {
        var time = DateTime.UtcNow;

        db.Execute($"UPDATE bbl_user SET last_login_time = @0 WHERE id = {userId}", time);

        return time;
    }

    public static void UpdateIpAndRegionByIp(Entities.BblUser bblUser, SavePoco db, IPAddress address) {
        var countryResponse = IpInfo.Country(address);
        if (countryResponse is null || countryResponse.RegisteredCountry.Name is null) return;
        var countryName = countryResponse.RegisteredCountry.Name;

        bblUser.LatestIp = address.ToString();

        var country = CountryInfo.FindByName(countryName);
        if (country is null)
            return;

        bblUser.Region = country.Value.NameShort;

        UpdateIpAndRegion(db, bblUser.Id, bblUser.Region, bblUser.LatestIp);
    }

    public static void UpdateIpAndRegionByIp(SavePoco db, long id, IPAddress address) {
        var countryResponse = IpInfo.Country(address);
        var country = CountryInfo.FindByName(countryResponse!.Country!.Name!);

        if (country is null) {
#if DEBUG
            throw new NullReferenceException(nameof(country));
#else
            return;
#endif
        }

        UpdateIpAndRegion(db, id, country.Value.NameShort, address.ToString());
    }

    public static void SetAcceptPatreonEmail(SavePoco db, long id, bool accept = true) {
        var sql = new Sql($@"
Update bbl_user 
Set patron_email_accept = {accept}
WHERE id = {id}
");

        db.Execute(sql);
    }

    public static void SetPatreonEmail(SavePoco db, long id, string email) {
        var sql = new Sql($@"
Update bbl_user 
Set patron_email = @0
WHERE id = {id}
", email);

        db.Execute(sql);
    }

    /// <param name="db"></param>
    /// <param name="region"></param>
    /// <param name="latest_ip"></param>
    private static void UpdateIpAndRegion(SavePoco db, long userId, string? region, string? latestIp) {
        db.Execute(@$"
UPDATE bbl_user 
SET region = '{region}', latest_ip = '{latestIp}'
WHERE id = {userId}
");
    }


    public static Response<BblPatron> GetBblPatron(Entities.BblUser bblUser, SavePoco db) {
        if (string.IsNullOrEmpty(bblUser.Email)) return Response<BblPatron>.Err;

        return db.SingleOrDefaultById<BblPatron>(bblUser.Email);
    }

    public static Response<bool> CheckPassword(SavePoco db, string username, string passwordHash) {
        var sql = new Sql(@"
SELECT username
FROM bbl_user
WHERE username = @0
AND password = @1
", username, passwordHash);

        return db.SingleOrDefault<Entities.BblUser>(sql) switch {
            { Status: EResponse.Ok } => true,
            _ => false
        };
    }

    public static Response<long> CheckPasswordGetId(SavePoco db, string username, string passwordHash) {
        var sql = new Sql(@"
SELECT id
FROM bbl_user
WHERE username = @0
AND password = @1
", username, passwordHash);

        return db.SingleOrDefault<Entities.BblUser>(sql) switch {
            { Status: EResponse.Ok } res =>
                res.Ok() is null ? Response<long>.Err : Response<long>.Ok(res.Ok()!.Id),
            _ => Response<long>.Err
        };
    }

    public class UserRank {
        [Column("global_rank")] public long globalRank { get; set; }
        [Column("country_rank")] public long CountryRank { get; set; }
    }
}