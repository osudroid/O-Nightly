using LamLibAllOver;
using OsuDroidLib.Validation;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class WebLogin : IWebLogin, IValuesAreGood {
    public int Math { get; set; }
    public Guid Token { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }

    public bool ValuesAreGood() {
        return Token != default
               && ValidationUsername.Validation(Email)
               && ValidationPassword.ValidationOldVersion(Password)
            ;
    }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
            Math.ToString(),
            Token.ToString(),
            Email ?? "",
            Password ?? "",
        });
    }
}