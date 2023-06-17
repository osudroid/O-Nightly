using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePasswd : Api2.IValuesAreGood, Api2.ISingleString  {
    public string? NewPasswd { get; set; }
    public string? OldPasswd { get; set; }
    public bool ValuesAreGood() {
        return ValidationPassword.ValidationNewVersion(NewPasswd)
            && ValidationPassword.ValidationOldVersion(OldPasswd)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewPasswd??"",
            OldPasswd??"",
        });
    }
}