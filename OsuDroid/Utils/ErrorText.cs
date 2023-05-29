namespace OsuDroid.Utils;

public static class ErrorText {
    public const string TokenInvalid = "Token Is Invalid";

    public static string HashBodyDataAreFalse(List<string> names) {
        return $"HashBodyData Are False Order By {SqlIn.Builder(names)}";
    }
}