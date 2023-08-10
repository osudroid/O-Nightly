using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebLoginWithUsername : Api2.IValuesAreGood, Api2.ISingleString {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                Math.ToString(),
                Token.ToString(),
                Username ?? "",
                Password ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        if (Token == default) return false;
        if (!ValidationUsername.Validation(Username)) return false;
        if (!ValidationPassword.ValidationOldVersion(Password)) return false;

        return true;
    }
}