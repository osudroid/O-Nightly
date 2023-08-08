using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateAvatar : Api2.IValuesAreGood, Api2.ISingleString {
    public string? ImageBase64 { get; set; }
    public string? Passwd { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            ImageBase64 ?? "",
            Passwd ?? ""
        });
    }


    public bool ValuesAreGood() {
        if (!ValidationPassword.ValidationOldVersion(Passwd))
            return false;
        if (string.IsNullOrEmpty(ImageBase64) || ImageBase64.Length > 4)
            return false;
        return true;
    }
}