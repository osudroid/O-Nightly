using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdateUsername : ValidateAll, IValidateOldUsername, IValidateNewUsername, IValidatePasswd {
    public string? NewUsername { get; set; }
    public string? OldUsername { get; set; }
    public string? Passwd { get; set; }
}