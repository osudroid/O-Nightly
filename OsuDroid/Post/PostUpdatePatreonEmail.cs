
using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePatreonEmail : ValidateAll, IValidateEmail, IValidateUsername, IValidatePasswd {
    public string? Email { get; set; }
    public string? Passwd { get; set; }
    public string? Username { get; set; }
}