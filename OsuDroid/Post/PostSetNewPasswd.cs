using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;

namespace OsuDroid.Post;

public sealed class PostSetNewPasswd : PostApi.IValuesAreGood, PostApi.ISingleString {
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
        if (!OsuDroidLib.Validation.ValidationPassword.ValidationNewVersion(NewPasswd))
            return false;
        
        return !(string.IsNullOrEmpty(NewPasswd)
                 || string.IsNullOrEmpty(Token)
                 || UserId < 0);
    }
}