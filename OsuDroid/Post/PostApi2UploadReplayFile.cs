using OsuDroid.Utils;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostApi2UploadReplayFile : OsuDroid.Post.Api2.IValuesAreGood, OsuDroid.Post.Api2.ISingleString,
                                               OsuDroid.Post.Api2.IPrintHashOrder {
    public string? MapHash { get; set; }
    public long ReplayId { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(MapHash),
            nameof(ReplayId)
        });
    }

    public string ToSingleString() {
        return Merge.ListToString(new object[] {
            MapHash ?? "", ReplayId
        });
    }

    public bool ValuesAreGood() {
        return
            string.IsNullOrEmpty(MapHash) == false
            && ReplayId > -1;
    }
}