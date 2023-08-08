using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePasswd : Api2.IValuesAreGood, Api2.ISingleString {
    public string? NewPasswd { get; set; }
    public string? OldPasswd { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            NewPasswd ?? "",
            OldPasswd ?? ""
        });
    }

    public bool ValuesAreGood() {
        return ValidationPassword.ValidationNewVersion(NewPasswd)
               && ValidationPassword.ValidationOldVersion(OldPasswd)
            ;
    }
}