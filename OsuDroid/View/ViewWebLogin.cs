using OsuDroidAttachment.Interface;

namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewWebLogin: IView, IHandlerOutput {
    public bool Work { get; set; }
    public bool UserOrPasswdOrMathIsFalse { get; set; }
    public bool UsernameFalse { get; set; }
    public bool EmailFalse { get; set; }
}