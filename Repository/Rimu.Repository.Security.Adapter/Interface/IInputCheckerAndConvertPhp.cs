namespace Rimu.Repository.Security.Adapter.Interface;

public interface IInputCheckerAndConvertPhp {
    public bool PasswdCleanAndCheckSize(string passwd);

    public string PhpStripsLashes(string value);

    /// <summary> Remove \r \n \t </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string TextFix(string str);

    public string RemoveMultipleSpaces(string str);

    public string FilterBlankStr(string str);
}