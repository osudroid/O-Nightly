using LamLibAllOver;
using OsuDroidLib.Validation;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class UpdateEmail : IUpdateEmail, IValuesAreGood {
    public string? NewEmail { get; set; }
    public string? OldEmail { get; set; }
    public string? Password { get; set; }
    
    public bool ValuesAreGood() {
        return ValidationEmail.Validation(NewEmail)
               && ValidationEmail.Validation(OldEmail)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            NewEmail ?? "",
            OldEmail ?? "",
            Password ?? "",
        });
    }
}