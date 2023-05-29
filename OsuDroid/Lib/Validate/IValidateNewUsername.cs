namespace OsuDroid.Lib.Validate;

public interface IValidateNewUsername {
    public static readonly Type Type = typeof(IValidateNewUsername);
    public string? NewUsername { get; set; }

    public static bool ValidateNewUsername(string? newUsername) {
        if (string.IsNullOrEmpty(newUsername))
            return false;
        return newUsername.Trim().Length >= 2;
    }

    public static bool ValidateNewUsername(IValidateNewUsername value) {
        return ValidateNewUsername(value.NewUsername);
    }
}