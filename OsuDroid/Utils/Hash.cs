using System.Security.Cryptography;

namespace OsuDroid.Utils;

internal sealed class Hash {
    [Obsolete("Obsolete")]
    public static string GetHashSha256(string text) {
        var bytes = Encoding.Unicode.GetBytes(text);
        var hashstring = new SHA256Managed();
        var hash = hashstring.ComputeHash(bytes);
        var hashString = string.Empty;
        foreach (var x in hash) hashString += string.Format("{0:x2}", x);
        return hashString;
    }

    [Obsolete("Obsolete")]
    public static string GetHashSha256(byte[] bytes) {
        var hashstring = new SHA256Managed();
        var hash = hashstring.ComputeHash(bytes);
        var hashString = string.Empty;
        foreach (var x in hash) hashString += string.Format("{0:x2}", x);
        return hashString;
    }
}