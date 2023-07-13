using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class SimpleToken : ISimpleToken, IValuesAreGood{
    public Guid Token { get; set; }

    public bool ValuesAreGood() {
        return Token != default;
    }

    public string ToSingleString() {
        return Token.ToString();
    }
}