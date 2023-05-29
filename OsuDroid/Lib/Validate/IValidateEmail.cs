namespace OsuDroid.Lib.Validate;

public interface IValidateEmail {
    public static readonly Type Type = typeof(IValidateEmail);
    public string? Email { get; }

    public static bool ValidateEmail(string? email) {
        if (string.IsNullOrEmpty(email))
            return false;
        return Utils.Email.ValidateEmail(email);
    }

    public static bool ValidateEmail(IValidateEmail value) {
        return ValidateEmail(value.Email);
    }
}