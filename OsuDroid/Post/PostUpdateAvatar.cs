using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateAvatar : Api2.IValuesAreGood, Api2.ISingleString {
    public string? ImageBase64 { get; set; }
    public string? Passwd { get; set; }


    public bool ValuesAreGood() {
        if (!OsuDroidLib.Validation.ValidationPassword.ValidationOldVersion(Passwd))
            return false;
        if (String.IsNullOrEmpty(ImageBase64) || ImageBase64.Length > 4)
            return false;
        return true;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            ImageBase64 ?? "",
            Passwd ?? ""
        });
    }
}