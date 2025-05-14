using System.Text;
using System.Text.RegularExpressions;
using Rimu.Repository.Security.Adapter.Interface;

namespace Rimu.Repository.Security.Domain;

public sealed class InputCheckerAndConvertPhp: IInputCheckerAndConvertPhp {
    private static readonly Regex RemoveMultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex FilterBlankStrRegex1 = new(@"/\s(?=\s)/", RegexOptions.Compiled);
    private static readonly Regex FilterBlankStrRegex2 = new("/[\'\" ]/", RegexOptions.Compiled);

    public InputCheckerAndConvertPhp() {
    }

    public bool PasswdCleanAndCheckSize(string passwd) {
        var passwdMd5 = PhpStripsLashes(passwd.Trim());
        return passwdMd5.Length == 32;
    }

    public string PhpStripsLashes(string value) {
        if (value.Contains('\\') == false)
            return value;

        StringBuilder output = new StringBuilder();

        int consecutiveCount = 0;
        foreach (char c in value)
        {
            if (c == '\\')
            {
                consecutiveCount++;
                if (consecutiveCount > 2)
                {
                    continue;
                }
            }
            else
            {
                consecutiveCount = 0;
            }

            output.Append(c);
        }

		return output.ToString().Replace("\\\\", "");
    }

    // private static Regex StripTagsRegex1 = new Regex("<[^>]*(>|$)", RegexOptions.Compiled);
    // private static Regex StripTagsRegex2 = new Regex(@"[\s\r\n]+", RegexOptions.Compiled);
    // private static string StripTags(string str) => StripTagsRegex2.Replace(StripTagsRegex1.Replace(str, String.Empty), String.Empty);

    /// <summary> Remove \r \n \t </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string TextFix(string str) {
        return str.Replace("\r", string.Empty)
                  .Replace("\n", string.Empty)
                  .Replace("\t", string.Empty);
    }

    public string RemoveMultipleSpaces(string str) {
        return RemoveMultipleSpacesRegex.Replace(str, " ");
    }

    public string FilterBlankStr(string str) {
        return FilterBlankStrRegex2.Replace(FilterBlankStrRegex1.Replace(str.Trim(), string.Empty), "_")
                                   .Replace(' ', '_')
                                   .Replace('\'', '_')
                                   .Replace('\"', '_');
    }
}