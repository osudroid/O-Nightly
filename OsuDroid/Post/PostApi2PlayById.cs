using OsuDroid.Utils;
using OsuDroid.View;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostApi2PlayById : PostApi.IValuesAreGood, PostApi.ISingleString, PostApi.IPrintHashOrder {
    public long PlayId { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(PlayId)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] { PlayId });
    }

    public bool ValuesAreGood() {
        return PlayId > -1;
    }
}