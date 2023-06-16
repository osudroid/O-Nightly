using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateAvatar : ValidateAll, IValidatePasswd {
    public string? ImageBase64 { get; set; }
    public string? Passwd { get; set; }
}