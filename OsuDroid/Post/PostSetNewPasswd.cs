using Microsoft.AspNetCore.Mvc;
using OsuDroid.View;

namespace OsuDroid.Post;

public sealed class PostSetNewPasswd : ApiTypes.IValuesAreGood, ApiTypes.ISingleString {
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
        return !(string.IsNullOrEmpty(NewPasswd)
                 || string.IsNullOrEmpty(Token)
                 || UserId < 0);
    }
}