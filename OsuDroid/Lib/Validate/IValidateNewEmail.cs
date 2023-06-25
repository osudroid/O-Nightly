using OsuDroid.Utils;

namespace OsuDroid.Lib.Validate;

public interface IValidateNewEmail {
    public static readonly Type Type = typeof(IValidateNewEmail);
    public string? NewEmail { get; set; }

    public static bool ValidateNewEmail(string? email) {
        if (string.IsNullOrEmpty(email))
            return false;
        return OsuDroidLib.Validation.ValidationEmail.Validation(email);
    }

    public static bool ValidateNewEmail(IValidateNewEmail value) {
        return ValidateNewEmail(value.NewEmail);
    }
}