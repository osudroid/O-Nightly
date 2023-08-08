using OsuDroidLib.Validation;

namespace OsuDroid.Lib.Validate;

public interface IValidateOldEmail {
    public static readonly Type Type = typeof(IValidateOldEmail);
    public string? OldEmail { get; set; }

    public static bool ValidateOldEmail(string? email) {
        if (string.IsNullOrEmpty(email))
            return false;
        return ValidationEmail.Validation(email);
    }

    public static bool ValidateOldEmail(IValidateOldEmail value) {
        return ValidateOldEmail(value.OldEmail);
    }
}