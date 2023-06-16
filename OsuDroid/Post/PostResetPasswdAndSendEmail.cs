using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostResetPasswdAndSendEmail : ValidateAll, IValidateEmail, IValidateUsername {
    public string? Email { get; set; }
    public string? Username { get; set; }
}