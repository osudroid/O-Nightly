namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewPlays {
    public bool Found { get; set; }
    public IReadOnlyList<Entities.PlayScore>? Scores { get; set; }
}