namespace OsuDroid.Lib.Validate;

public interface IValidateOldUsername {
    public static readonly Type Type = typeof(IValidateOldUsername);
    public string? OldUsername { get; set; }

    public static bool ValidateOldUsername(string? oldUsername) {
        if (string.IsNullOrEmpty(oldUsername))
            return false;
        return oldUsername.Trim().Length >= 2;
    }

    public static bool ValidateOldUsername(IValidateOldUsername value) {
        return ValidateOldUsername(value.OldUsername);
    }
}