namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewUserInfo: IView {
    public long Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public DateTime RegistTime { get; set; }
    public string? Region { get; set; }
    public bool Active { get; set; }
    public bool Supporter { get; set; }
    public bool Banned { get; set; }
    public bool RestrictMode { get; set; }
}