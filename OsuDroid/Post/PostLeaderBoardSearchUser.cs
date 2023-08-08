using OsuDroid.Utils;
using OsuDroidLib.Lib;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostLeaderBoardSearchUser : Api2.IValuesAreGood, Api2.ISingleString, Api2.IPrintHashOrder,
                                                ILogRequestJsonPrint {
    private string? _region;
    public long Limit { get; set; }
    public string? Query { get; set; }

    public string Region {
        get => _region ?? "";
        set => _region = value == "All" || value == "" ? "all" : value;
    }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(Limit),
            nameof(Query),
            nameof(Region)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
            Limit.ToString(),
            Query ?? "",
            Region
        });
    }

    public bool ValuesAreGood() {
        return
            Limit > 0
            && !string.IsNullOrEmpty(Query);
    }

    public Option<CountryInfo.Country> GetRegionAsCountry() {
        return CountryInfo.FindByName(Region ?? "");
    }

    public bool IsRegionAll() {
        return Region == "all";
    }
}