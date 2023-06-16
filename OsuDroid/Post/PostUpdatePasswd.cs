using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePasswd : ValidateAll, IValidateOldPasswd, IValidateNewPasswd {
    public string? NewPasswd { get; set; }
    public string? OldPasswd { get; set; }
}