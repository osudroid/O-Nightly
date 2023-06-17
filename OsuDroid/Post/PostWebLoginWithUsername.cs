using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebLoginWithUsername : Api2.IValuesAreGood, Api2.ISingleString {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Username { get; set; }
    public string? Passwd { get; set; }

    public bool ValuesAreGood() {
        return Token != default
               && ValidationUsername.Validation(Username)
               && ValidationPassword.ValidationOldVersion(Passwd)
            ;


    }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            Math.ToString(),
            Token.ToString(),
            Username??"",
            Passwd??"",
        });
    }
}