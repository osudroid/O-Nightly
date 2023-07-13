namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ViewApiUpdateInfoV2: IView {
    public long VersionCode { get; set; }
    public string? Link { get; set; }
    public string? Changelog { get; set; }
}