namespace OsuDroid.Lib.Validate;

public interface IValidateEmail {
    public static readonly Type Type = typeof(IValidateEmail);
    public string? Email { get; }

    public static bool ValidateEmail(string? email) {
        if (string.IsNullOrEmpty(email))
            return false;
        return OsuDroidLib.Validation.ValidationEmail.Validation(email);
    }

    public static bool ValidateEmail(IValidateEmail value) {
        return ValidateEmail(value.Email);
    }
}