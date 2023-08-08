using Microsoft.AspNetCore.Mvc;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

public sealed class PostSetNewPasswd : Api2.IValuesAreGood, Api2.ISingleString {
    [FromBody] public string? NewPasswd { get; set; }
    [FromBody] public string? Token { get; set; }
    [FromBody] public long UserId { get; set; }

    public string ToSingleString() {
        return Merge.ObjectsToString(new[] {
            NewPasswd ?? "",
            Token ?? "",
            UserId.ToString()
        });
    }

    public bool ValuesAreGood() {
        if (!ValidationPassword.ValidationNewVersion(NewPasswd))
            return false;

        return !(string.IsNullOrEmpty(NewPasswd)
                 || string.IsNullOrEmpty(Token)
                 || UserId < 0);
    }
}