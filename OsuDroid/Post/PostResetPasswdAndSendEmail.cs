using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostResetPasswdAndSendEmail : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Email { get; set; }
    public string? Username { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                Email ?? "",
                Username ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        if (!ValidationUsername.Validation(Username))
            Username = "";
        if (!ValidationEmail.Validation(Email))
            Email = "";

        if (this is { Username: "", Email: "" })
            return false;

        return true;
    }
}