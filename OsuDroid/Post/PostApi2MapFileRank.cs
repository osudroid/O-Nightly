using OsuDroid.Utils;
using OsuDroid.Class;

namespace OsuDroid.Post;

public class PostApi2MapFileRank : PostApi.IValuesAreGood, PostApi.ISingleString, PostApi.IPrintHashOrder {
    public string? Filename { get; set; }
    public string? FileHash { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(Filename),
            nameof(FileHash)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
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