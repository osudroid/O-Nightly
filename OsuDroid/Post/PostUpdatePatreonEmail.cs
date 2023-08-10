using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePatreonEmail : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Username { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                Email ?? "",
                Password ?? "",
                Username ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        return ValidationEmail.Validation(Email)
               && ValidationPassword.ValidationOldVersion(Password)
               && ValidationUsername.Validation(Username)
            ;
    }
}