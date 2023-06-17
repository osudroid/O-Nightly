using OsuDroid.Model;
using OsuDroid.Utils;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostPushPlayStart : Submit.ScoreProp, OsuDroid.Post.Api2.IValuesAreGood, OsuDroid.Post.Api2.ISingleString,
                                        PostApi.IPrintHashOrder {
    public string? Filename { get; set; }
    public string? FileHash { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(Filename),
            nameof(FileHash)
        });
    }

    public string ToSingleString() {
        return Merge
            .ObjectsToString(new Object[] {
                Filename ?? "",
                FileHash ?? ""
            });
    }

    public bool ValuesAreGood() {
        return string.IsNullOrEmpty(Filename) != true
               && string.IsNullOrEmpty(FileHash) != true
            ;
    }
}