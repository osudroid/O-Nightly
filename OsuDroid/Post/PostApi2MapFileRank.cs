using OsuDroid.Utils;

namespace OsuDroid.Post;

public class PostApi2MapFileRank : Api2.IValuesAreGood, Api2.ISingleString, Api2.IPrintHashOrder {
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