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

    public static bool PasswordEqual(Entities.UserInfo userInfo, string passwd) {
        return userInfo.Password == LamLibAllOver.MD5.Hash(passwd + Env.PasswdSeed).ToLower();
    }
}