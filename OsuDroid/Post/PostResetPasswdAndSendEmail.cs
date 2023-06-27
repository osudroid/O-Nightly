using OsuDroid.Lib.Validate;
using Patreon.NET;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostResetPasswdAndSendEmail : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Email { get; set; }
    public string? Username { get; set; }

    public bool ValuesAreGood() {
        if (!OsuDroidLib.Validation.ValidationUsername.Validation(Username))
            this.Username = "";
        if (!OsuDroidLib.Validation.ValidationEmail.Validation(Email))
            this.Email = "";

        if (this is { Username: "", Email: "" })
            return false;

        return true;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            Email ?? "",
            Username ?? "",
        });
    }
}