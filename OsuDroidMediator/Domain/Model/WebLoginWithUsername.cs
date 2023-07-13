using LamLibAllOver;
using OsuDroidMediator.Domain.Interface;
using OsuDroidLib.Validation;

namespace OsuDroidMediator.Domain.Model; 

public class WebLoginWithUsername: IWebLoginWithUsername, IValuesAreGood {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public bool ValuesAreGood() {
        return Token != default
               && ValidationUsername.Validation(Username)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }
    
    public string ToSingleString() {
        return Merge.ListToString(new[] {
            Math.ToString(),
            Token.ToString(),
            Username ?? "",
            Password ?? "",
        });
    }
}