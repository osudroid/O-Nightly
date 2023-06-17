using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateUsername : Api2.IValuesAreGood, Api2.ISingleString {
    public string? NewUsername { get; set; }
    public string? OldUsername { get; set; }
    public string? Passwd { get; set; }
    public bool ValuesAreGood() {
        return ValidationUsername.Validation(NewUsername)
               && ValidationUsername.Validation(OldUsername)
               && ValidationPassword.ValidationOldVersion(Passwd)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewUsername??"",
            OldUsername??"",
            Passwd??"",
        });
    }
}