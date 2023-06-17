namespace OsuDroid.Class;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewUserRankTimeLine {
    public long UserId { get; set; }
    public IReadOnlyList<RankTimeLineValue>? List { get; set; }

    public class RankTimeLineValue {
        public DateTime Date { get; set; }
        public long Score { get; set; }
        public long Rank { get; set; }
    }
}