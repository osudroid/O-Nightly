using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostCreateDropAccountToken : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Password { get; set; }

    public string ToSingleString() {
        return Password ?? "";
    }

    public bool ValuesAreGood() {
        return ValidationPassword.ValidationOldVersion(Password);
    }
}