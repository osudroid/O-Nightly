using OsuDroidAttachment.Interface;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebLogin : Api2.IValuesAreGood, Api2.ISingleString, IInput {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            Math.ToString(),
            Token.ToString(),
            Email ?? "",
            Password ?? ""
        });
    }

    public bool ValuesAreGood() {
        return Token != default
               && ValidationUsername.Validation(Email)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }
}