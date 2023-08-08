namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ViewCreateApi2TokenResult : IView {
    public required Guid Token { get; set; }
    public required bool UsernameFalse { get; set; }
    public required bool PasswdFalse { get; set; }
}