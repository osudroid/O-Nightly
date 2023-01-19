namespace OsuDroid.Lib.Validate;

public interface IValidateUsername {
    public static readonly Type Type = typeof(IValidateUsername);
    public string? Username { get; set; }

    public static bool ValidateUsername(string? username) {
        if (string.IsNullOrEmpty(username))
            return false;
        return username.Trim().Length >= 2;
    }

    public static bool ValidateUsername(IValidateUsername value) {
        return ValidateUsername(value.Username);
    }
}