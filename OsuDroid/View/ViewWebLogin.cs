namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewWebLogin: IView {
    public bool Work { get; set; }
    public bool UserOrPasswdOrMathIsFalse { get; set; }
    public bool UsernameExist { get; set; }
    public bool EmailExist { get; set; }
}