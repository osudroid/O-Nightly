namespace OsuDroid.Lib.Validate;

public interface IValidatePasswd {
    public static readonly Type Type = typeof(IValidatePasswd);
    public string? Passwd { get; set; }

    public static bool ValidatePasswd(string? passwd) {
        return string.IsNullOrEmpty(passwd) == false && passwd.Length > 5;
    }

    public static bool ValidatePasswd(IValidatePasswd value) {
        return ValidatePasswd(value.Passwd);
    }
}