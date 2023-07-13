using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public record class None: IValuesAreGood {
    public bool ValuesAreGood() => true;
    
    public static None NoneValue { get; } = new None();
}