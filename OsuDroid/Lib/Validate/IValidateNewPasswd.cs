namespace OsuDroid.Lib.Validate;

public interface IValidateNewPasswd {
    public static readonly Type Type = typeof(IValidateNewPasswd);
    public string? NewPasswd { get; }

    public static bool ValidateNewPasswd(string? newPasswd) {
        return string.IsNullOrEmpty(newPasswd) == false && newPasswd.Length > 5;
    }

    public static bool ValidateNewPasswd(IValidateNewPasswd value) {
        return ValidateNewPasswd(value.NewPasswd);
    }
}