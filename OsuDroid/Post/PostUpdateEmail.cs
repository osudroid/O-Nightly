using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateEmail : Api2.IValuesAreGood, Api2.ISingleString {
    public string? NewEmail { get; set; }
    public string? OldEmail { get; set; }
    public string? Passwd { get; set; }
    public bool ValuesAreGood() {
        return ValidationEmail.Validation(NewEmail)
            && ValidationEmail.Validation(OldEmail)
            && ValidationPassword.ValidationOldVersion(Passwd)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewEmail ?? "",
            OldEmail ?? "",
            Passwd ?? "",
        });
    }
}