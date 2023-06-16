using OsuDroid.Lib.Validate;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostWebRegister : ValidateAll, IValidatePasswd, IValidateUsername {
    public string? Email { get; set; }
    public int MathRes { get; set; }
    public Guid MathToken { get; set; }
    public string? Region { get; set; }
    public string? Passwd { get; set; }
    public string? Username { get; set; }
}