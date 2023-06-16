using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebLogin : ValidateAll, IValidatePasswd, IValidateEmail {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Email { get; set; }
    public string? Passwd { get; set; }
}