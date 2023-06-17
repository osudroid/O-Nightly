using OsuDroid.Model;
using OsuDroid.Utils;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostPushPlay : Submit.ScoreProp, OsuDroid.Post.Api2.IValuesAreGood, OsuDroid.Post.Api2.ISingleString,
                                   OsuDroid.Post.Api2.IPrintHashOrder {
    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(Mode),
            nameof(Mark),
            nameof(Id),
            nameof(Score),
            nameof(Combo),
            nameof(Uid),
            nameof(Geki),
            nameof(Perfect),
            nameof(Katu),
            nameof(Good),
            nameof(Bad),
            nameof(Miss),
            nameof(Accuracy)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
            Mode ?? "",
            Mark ?? "",
            Id,
            Score,
            Combo,
            Uid,
            Geki,
            Perfect,
            Katu,
            Good,
            Bad,
            Miss,
            Accuracy
        });
    }

    public bool ValuesAreGood() {
        return
            string.IsNullOrEmpty(Mode) != true &&
            string.IsNullOrEmpty(Mark) != true &&
            Id != -1 &&
            Score != -1 &&
            Combo != -1 &&
            Uid != -1 &&
            Geki != -1 &&
            Perfect != -1 &&
            Katu != -1 &&
            Good != -1 &&
            Bad != -1 &&
            Miss != -1 &&
            Accuracy != -1
            ;
    }
}