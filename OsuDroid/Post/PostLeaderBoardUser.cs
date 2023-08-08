using OsuDroid.Utils;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostLeaderBoardUser : Api2.IValuesAreGood, Api2.ISingleString, Api2.IPrintHashOrder {
    public long UserId { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(UserId)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] { UserId });
    }

    public bool ValuesAreGood() {
        return UserId > -1;
    }
}