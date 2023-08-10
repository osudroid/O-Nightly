using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePasswd : Api2.IValuesAreGood, Api2.ISingleString {
    public string? NewPassword { get; set; }
    public string? OldPassword { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                NewPassword ?? "",
                OldPassword ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        return ValidationPassword.ValidationNewVersion(NewPassword)
               && ValidationPassword.ValidationOldVersion(OldPassword)
            ;
    }
}