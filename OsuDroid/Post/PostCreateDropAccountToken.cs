using OsuDroid.View;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostCreateDropAccountToken : PostApi.IValuesAreGood, PostApi.ISingleString {
    public string? Password { get; set; }

    public bool ValuesAreGood() {
        return OsuDroidLib.Validation.ValidationPassword.ValidationOldVersion(Password);
    }

    public string ToSingleString() => Password ?? "";
}