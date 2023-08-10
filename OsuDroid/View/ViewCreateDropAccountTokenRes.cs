namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewCreateDropAccountTokenRes : IView, PostApi.IValuesAreGood, PostApi.ISingleString {
    public bool Work { get; set; }
    public bool PasswordFalse { get; set; }
    public bool CookieDead { get; set; }
    public bool ElseError { get; set; }

    public string ToSingleString() {
        return Merge.ListToString(new[] {
                Work.ToString(),
                PasswordFalse.ToString(),
                CookieDead.ToString(),
                ElseError.ToString()
            }
        );
    }

    public bool ValuesAreGood() {
        return true;
    }

    public static ViewCreateDropAccountTokenRes NoError() {
        return new ViewCreateDropAccountTokenRes { Work = true };
    }

    public static ViewCreateDropAccountTokenRes PasswordIsFalse() {
        return new ViewCreateDropAccountTokenRes { Work = false, PasswordFalse = true };
    }

    public static ViewCreateDropAccountTokenRes CookieIsDead() {
        return new ViewCreateDropAccountTokenRes { Work = false, CookieDead = true };
    }

    public static ViewCreateDropAccountTokenRes HasElseError() {
        return new ViewCreateDropAccountTokenRes { Work = false, ElseError = true };
    }
}