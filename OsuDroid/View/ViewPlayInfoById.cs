namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewPlayInfoById: IView {
    public ViewPlayScore? Score { get; set; }
    public string? Username { get; set; }
    public string? Region { get; set; }
}