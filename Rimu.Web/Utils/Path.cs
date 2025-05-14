namespace Rimu.Web.Utils;

public static class Path {
    public static string Process() {
        return Environment.ProcessPath!.Remove(Environment.ProcessPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
    }
}