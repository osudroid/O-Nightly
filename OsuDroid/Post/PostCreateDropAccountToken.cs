using OsuDroid.View;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostCreateDropAccountToken : ApiTypes.IValuesAreGood, ApiTypes.ISingleString {
    public string? Password { get; set; }

    public bool ValuesAreGood() => !string.IsNullOrEmpty(Password);

    public string ToSingleString() => Password??"";
}