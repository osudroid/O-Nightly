using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PostCreateApi2Token : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Username { get; set; }
    public string? Password { get; set; }


    public string ToSingleString() {
        return Merge.ListToString(new List<string> { Username ?? "", Password ?? "" });
    }

    public bool ValuesAreGood() {
        if (!ValidationUsername.Validation(Username))
            return false;
        if (!ValidationPassword.ValidationOldVersion(Password))
            return false;
        return true;
    }
}