using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebLoginWithUsername : ValidateAll, IValidatePasswd, IValidateUsername {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Passwd { get; set; }
    public string? Username { get; set; }
}