using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateEmail : ValidateAll, IValidateOldEmail, IValidateNewEmail, IValidatePasswd {
    public string? NewEmail { get; set; }
    public string? OldEmail { get; set; }
    public string? Passwd { get; set; }
}