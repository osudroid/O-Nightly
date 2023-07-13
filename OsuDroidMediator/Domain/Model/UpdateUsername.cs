using LamLibAllOver;
using OsuDroidMediator.Domain.Interface;
using OsuDroidLib.Validation;

namespace OsuDroidMediator.Domain.Model; 

public class UpdateUsername : IUpdateUsername, IValuesAreGood {
    public string? NewUsername { get; set; }
    public string? OldUsername { get; set; }
    public string? Password { get; set; }
    
    public bool ValuesAreGood() {
        return ValidationUsername.Validation(NewUsername)
               && ValidationUsername.Validation(OldUsername)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewUsername ?? "",
            OldUsername ?? "",
            Password ?? "",
        });
    }
}