namespace OsuDroidLib.Validation;

public class ValidationUsername {
    public static bool Validation(string? username) {
        if (String.IsNullOrEmpty(username))
            return false;
        if (username.IndexOf(' ') == -1)
            return false;
        return true;
    }
}