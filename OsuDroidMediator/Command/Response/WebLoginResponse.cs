namespace OsuDroidMediator.Command.Response; 

public class WebLoginResponse {
    public bool Work { get; set; }
    public bool UserOrPasswdOrMathIsFalse { get; set; }
    public bool UsernameExist { get; set; }
    public bool EmailExist { get; set; }
}