namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewPlays: IView {
    public bool Found { get; set; }
    public IReadOnlyList<ViewPlayScore>? Scores { get; set; }
}