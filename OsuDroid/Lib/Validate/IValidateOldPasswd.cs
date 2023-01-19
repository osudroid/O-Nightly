namespace OsuDroid.Lib.Validate;

public interface IValidateOldPasswd {
    public static readonly Type Type = typeof(IValidateOldPasswd);
    public string? OldPasswd { get; }

    public static bool ValidateOldPasswd(string? oldPasswd) {
        return string.IsNullOrEmpty(oldPasswd) == false && oldPasswd.Length > 5;
    }

    public static bool ValidateOldPasswd(IValidateOldPasswd value) {
        return ValidateOldPasswd(value.OldPasswd);
    }
}