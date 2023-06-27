using Patreon.NET;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PostCreateApi2Token : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Username { get; set; }
    public string? Passwd { get; set; }

    public bool ValuesAreGood() {
        if (!OsuDroidLib.Validation.ValidationUsername.Validation(Username))
            return false;
        if (!OsuDroidLib.Validation.ValidationPassword.ValidationOldVersion(Passwd))
            return false;
        return true;
    }


    public string ToSingleString() {
        return Merge.ListToString(new List<string>() { Username ?? "", Passwd ?? "" });
    }
}