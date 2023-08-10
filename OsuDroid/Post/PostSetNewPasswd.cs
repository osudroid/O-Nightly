using Microsoft.AspNetCore.Mvc;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

public sealed class PostSetNewPasswd : Api2.IValuesAreGood, Api2.ISingleString {
    [FromBody] public string? NewPassword { get; set; }
    [FromBody] public string? Token { get; set; }
    [FromBody] public long UserId { get; set; }

    public string ToSingleString() {
        return Merge.ObjectsToString(new[] {
                NewPassword ?? "",
                Token ?? "",
                UserId.ToString()
            }
        );
    }

    public bool ValuesAreGood() {
        if (!ValidationPassword.ValidationNewVersion(NewPassword))
            return false;

        return !(string.IsNullOrEmpty(NewPassword)
                 || string.IsNullOrEmpty(Token)
                 || UserId < 0);
    }
}