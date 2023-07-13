using OsuDroid.Utils;
using OsuDroid.View;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostRecentPlays : PostApi.IValuesAreGood, PostApi.ISingleString, PostApi.IPrintHashOrder {
    public string? FilterPlays { get; set; }
    public string? OrderBy { get; set; }
    public int Limit { get; set; }
    public int StartAt { get; set; }

    public string PrintHashOrder() {
        return ErrorText.HashBodyDataAreFalse(new List<string> {
            nameof(FilterPlays),
            nameof(OrderBy),
            nameof(Limit),
            nameof(StartAt)
        });
    }

    public string ToSingleString() {
        return Merge.ObjectsToString(new object[] {
            FilterPlays ?? "",
            OrderBy ?? "",
            Limit,
            StartAt
        });
    }

    public bool ValuesAreGood() {
        return ValidateFilterPlays()
               && ValidateOrderBy()
               && ValidateLimit()
               && ValidateStartAt();
    }

    private bool ValidateFilterPlays() {
        return FilterPlays switch {
            "Any"
                or "XSS_Plays"
                or "SS_Plays"
                or "XS_Plays"
                or "S_Plays"
                or "A_Plays"
                or "B_Plays"
                or "C_Plays"
                or "D_Plays"
                or "Accuracy_100"
                => true,
            _ => false
        };
    }

    private bool ValidateOrderBy() {
        return OrderBy switch {
            "Time_ASC"
                or "Time_DESC"
                or "Score_ASC"
                or "Score_DESC"
                or "Combo_ASC"
                or "Combo_DESC"
                or "50_ASC"
                or "50_DESC"
                or "100_ASC"
                or "100_DESC"
                or "300_ASC"
                or "300_DESC"
                or "Miss_ASC"
                or "Miss_DESC"
                => true,
            _ => false
        };
    }

    private bool ValidateLimit() {
        if (Limit > 100 || Limit < 1)
            return false;
        return true;
    }

    private bool ValidateStartAt() {
        if (StartAt < 0)
            return false;
        return true;
    }
}