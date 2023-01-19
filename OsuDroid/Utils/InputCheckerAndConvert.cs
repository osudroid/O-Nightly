using System.Text.RegularExpressions;

namespace OsuDroid.Utils;

public static class InputCheckerAndConvert {
    private static readonly Regex RemoveMultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex FilterBlankStrRegex1 = new(@"/\s(?=\s)/", RegexOptions.Compiled);
    private static readonly Regex FilterBlankStrRegex2 = new("/[\'\" ]/", RegexOptions.Compiled);

    public static bool PasswdCleanAndCheckSize(string passwd) {
        var passwdMd5 = PhpStripsLashes(passwd.Trim());
        return passwdMd5.Length == 32;
    }

    public static string PhpStripsLashes(string value) {
        if (value.Contains("\\") == false)
            return value;

        if (value.Contains(@"\\\") == false) return value.Replace("\\", "");

        var res = new StringBuilder(value.Length);

        for (var i = 0; i < value.Length; i++) {
            if (value[i] != '\\') {
                res.Append(value[i]);
                continue;
            }

            if (i + 2 ! < value.Length)
                break;

            if (value[i + 1] != '\\' || value[i + 2] != '\\')
                continue;

            i += 2;
            res.Append('\\');
        }

        return res.ToString();
    }

    // private static Regex StripTagsRegex1 = new Regex("<[^>]*(>|$)", RegexOptions.Compiled);
    // private static Regex StripTagsRegex2 = new Regex(@"[\s\r\n]+", RegexOptions.Compiled);
    // private static string StripTags(string str) => StripTagsRegex2.Replace(StripTagsRegex1.Replace(str, String.Empty), String.Empty);

    /// <summary> Remove \r \n \t </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string TextFix(string str) {
        return str.Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty);
    }

    public static string RemoveMultipleSpaces(string str) {
        return RemoveMultipleSpacesRegex.Replace(str, " ");
    }

    public static string FilterBlankStr(string str) {
        return FilterBlankStrRegex2.Replace(FilterBlankStrRegex1.Replace(str.Trim(), string.Empty), "_")
            .Replace(' ', '_')
            .Replace('\'', '_')
            .Replace('\"', '_');
    }
}