using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateUsername : Api2.IValuesAreGood, Api2.ISingleString {
    public string? NewUsername { get; set; }
    public string? OldUsername { get; set; }
    public string? Password { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                NewUsername ?? "",
                OldUsername ?? "",
                Password ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        return ValidationUsername.Validation(NewUsername)
               && ValidationUsername.Validation(OldUsername)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }
}