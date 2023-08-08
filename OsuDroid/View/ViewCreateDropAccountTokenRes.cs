namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewCreateDropAccountTokenRes: IView, PostApi.IValuesAreGood, PostApi.ISingleString {
    public bool Work { get; set; }
    public bool PasswordFalse { get; set; }
    public bool CookieDead { get; set; }
    public bool ElseError { get; set; }

    public static ViewCreateDropAccountTokenRes NoError() => new() { Work = true };
    public static ViewCreateDropAccountTokenRes PasswordIsFalse() => new() { Work = false, PasswordFalse = true };
    public static ViewCreateDropAccountTokenRes CookieIsDead() => new() { Work = false, CookieDead = true };
    public static ViewCreateDropAccountTokenRes HasElseError() => new() { Work = false, ElseError = true };
    public bool ValuesAreGood() => true;

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            Work.ToString(),
            PasswordFalse.ToString(),
            CookieDead.ToString(),
            ElseError.ToString(),
        });
    }
}