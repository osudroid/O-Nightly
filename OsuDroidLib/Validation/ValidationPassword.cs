namespace OsuDroidLib.Validation;

public static class ValidationPassword {
    public static bool ValidationOldVersion(string? password) {
        if (string.IsNullOrEmpty(password))
            return false;
        if (password.IndexOf(' ') != -1)
            return false;

        return true;
    }

    public static bool ValidationNewVersion(string? password) {
        if (string.IsNullOrEmpty(password))
            return false;
        if (password.IndexOf(' ') != -1)
            return false;
        if (password.Length < Setting.Password_MinLength!.Value)
            return false;

        return true;
    }
}