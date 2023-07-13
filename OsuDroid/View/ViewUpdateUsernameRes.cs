namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewUpdateUsernameRes: IView {
    public bool HasWork { get; set; }
    public int WaitTimeForNextDayToUpdate { get; set; }
}