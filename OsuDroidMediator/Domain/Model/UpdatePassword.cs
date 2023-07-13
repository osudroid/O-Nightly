using LamLibAllOver;
using OsuDroidLib.Validation;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class UpdatePassword : IUpdatePassword, IValuesAreGood {
    public string? NewPassword { get; set; }
    public string? OldPassword { get; set; }
    
    public bool ValuesAreGood() {
        return ValidationPassword.ValidationNewVersion(NewPassword)
               && ValidationPassword.ValidationOldVersion(OldPassword)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewPassword ?? "",
            OldPassword ?? "",
        });
    }
}