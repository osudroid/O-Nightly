using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class WebRegister: IWebRegister, IValuesAreGood {
    public string? Email { get; set; }
    public int MathResult { get; set; }
    public Guid MathToken { get; set; }
    public string? Region { get; set; }
    public string? Password { get; set; }
    public string? Username { get; set; }
    
    public bool ValuesAreGood() {
        throw new NotImplementedException();
    }
}