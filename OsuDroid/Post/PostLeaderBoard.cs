using OsuDroid.Utils;
using OsuDroidLib.Lib;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostLeaderBoard : Api2.IValuesAreGood, Api2.ISingleString, Api2.IPrintHashOrder {
    private string? _region;
    public int Limit { get; set; }

    public string? Region {
        get => _region;
        set => _region = value == "All" || value == "" ? "all" : value;
    }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
                nameof(Limit),
                nameof(Region)
            }
        );
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
                Limit,
                Region ?? ""
            }
        );
    }

    public bool ValuesAreGood() {
        if (Limit <= 0) return false;
        if (Region == "all") return true;
        return GetRegionAsCountry().IsSet();
    }

    public bool IsRegionAll() {
        return Region == "all";
    }

    public Option<CountryInfo.Country> GetRegionAsCountry() {
        return CountryInfo.FindByName(Region ?? "");
    }
}