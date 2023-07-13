using LamLibAllOver;
using OsuDroidLib.Validation;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class UpdatePatreonEmail : IUpdatePatreonEmail, IValuesAreGood {
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Username { get; set; }
    
    public bool ValuesAreGood() {
        return ValidationEmail.Validation(Email)
               && ValidationPassword.ValidationOldVersion(Password)
               && ValidationUsername.Validation(Username)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            Email ?? "",
            Password ?? "",
            Username ?? "",
        });
    }
}