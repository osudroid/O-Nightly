
using OsuDroid.Lib.Validate;
using OsuDroidLib.Validation;

namespace OsuDroid.Post;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PostUpdatePatreonEmail : Api2.IValuesAreGood, Api2.ISingleString {
    public string? Email { get; set; }
    public string? Passwd { get; set; }
    public string? Username { get; set; }
    public bool ValuesAreGood() {
        return ValidationEmail.Validation(Email) 
               && ValidationPassword.ValidationOldVersion(Passwd) 
               && ValidationUsername.Validation(Username)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            Email??"",
            Passwd??"",
            Username??"",
        });
    }
}