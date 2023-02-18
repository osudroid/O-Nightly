using System.Net;
using MaxMind.GeoIP2.Responses;
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

    public static Result<Option<Entities.BblUser>, string> GetUserById(SavePoco db, long id) {
        var sql = new Sql($"SELECT * FROM bbl_user WHERE id = {id} LIMIT 1");

        return db.FirstOrDefault<Entities.BblUser>(sql).Map(x => Option<Entities.BblUser>.NullSplit(x));
    }

    public static DateTime UpdateLastLoginTime(Entities.BblUser bblUser, SavePoco db) {
        return UpdateLastLoginTime(db, bblUser.Id);
    }

    public static DateTime UpdateLastLoginTime(SavePoco db, long userId) {
        var time = DateTime.UtcNow;

        db.Execute($"UPDATE bbl_user SET last_login_time = @0 WHERE id = {userId}", time);

        return time;
    }

    public static ResultErr<string> UpdateIpAndRegionByIp(Entities.BblUser bblUser, SavePoco db, IPAddress address) {
        CountryResponse? countryResponse = IpInfo.Country(address);
        if (countryResponse is null || countryResponse.RegisteredCountry.Name is null) 
            return ResultErr<string>.Err("County Not Found By IpAddress");
        var countryName = countryResponse.RegisteredCountry.Name;

        bblUser.LatestIp = address.ToString();

        var optionCountry = CountryInfo.FindByName(countryName);
        if (optionCountry.IsSet() == false)
            return ResultErr<string>.Err("CountryInfo Not Found");

        bblUser.Region = optionCountry.Unwrap().NameShort;

        return UpdateIpAndRegion(db, bblUser.Id, bblUser.Region, bblUser.LatestIp);
    }

    public static ResultErr<string> UpdateIpAndRegionByIp(SavePoco db, long id, IPAddress address) {
        var countryResponse = IpInfo.Country(address);
        var optionCountry = CountryInfo.FindByName(countryResponse!.Country!.Name!);

        if (optionCountry.IsSet() == false) {
            return ResultErr<string>.Err("CountryInfo Not Found");
        }

        var country = optionCountry.Unwrap();
        return UpdateIpAndRegion(db, id, country.NameShort, address.ToString());
    }

    public static ResultErr<string> SetAcceptPatreonEmail(SavePoco db, long id, bool accept = true) {
        var sql = new Sql($@"
Update bbl_user 
Set patron_email_accept = {accept}
WHERE id = {id}
");

        return db.Execute(sql);
    }

    public static ResultErr<string> SetPatreonEmail(SavePoco db, long id, string email) {
        var sql = new Sql($@"
Update bbl_user 
Set patron_email = @0
WHERE id = {id}
", email);

        return db.Execute(sql);
    }

    /// <param name="db"></param>
    /// <param name="region"></param>
    /// <param name="latest_ip"></param>
    private static ResultErr<string> UpdateIpAndRegion(SavePoco db, long userId, string? region, string? latestIp) {
        return db.Execute(@$"
UPDATE bbl_user 
SET region = '{region}', latest_ip = '{latestIp}'
WHERE id = {userId}
");
    }


    public static Result<OsuDroidLib.Database.Entities.BblPatron, string> GetBblPatron(Entities.BblUser bblUser, SavePoco db) {
        if (string.IsNullOrEmpty(bblUser.Email))
            return Result<OsuDroidLib.Database.Entities.BblPatron, string>.Err($"{nameof(bblUser.Email)} IS NULL");
        
        return db.SingleOrDefaultById<OsuDroidLib.Database.Entities.BblPatron>(bblUser.Email);
    }

    public static Result<bool, string> CheckPassword(SavePoco db, string username, string passwordHash) {
        var sql = new Sql(@"
SELECT username
FROM bbl_user
WHERE username = @0
AND password = @1
", username, passwordHash);

        return db.SingleOrDefault<Entities.BblUser>(sql).Map(x => x is not null );
    }

    public static Result<Option<long>, string> CheckPasswordGetId(SavePoco db, string username, string passwordHash) {
        var sql = new Sql(@"
SELECT id
FROM bbl_user
WHERE username = @0
AND password = @1
", username, passwordHash);

        return db.SingleOrDefault<Entities.BblUser>(sql)
            .Map(x => x is not null? Option<long>.With(x.Id): Option<long>.Empty);
    }

    public static ResultErr<string> DeleteBblUser(SavePoco db, long userId) {
        return db.Execute(@$"Delete FROM bbl_user WHERE id = {userId}");
    }

    public class UserRank {
        [Column("global_rank")] public long globalRank { get; set; }
        [Column("country_rank")] public long CountryRank { get; set; }
    }
}