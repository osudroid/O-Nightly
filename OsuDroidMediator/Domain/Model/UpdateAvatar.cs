using LamLibAllOver;
using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public class UpdateAvatar: IUpdateAvatar, IValuesAreGood {
    public string? ImageBase64 { get; set; }
    public string? Password { get; set; }
    
    public bool ValuesAreGood() {
        if (!OsuDroidLib.Validation.ValidationPassword.ValidationOldVersion(Password))
            return false;
        if (String.IsNullOrEmpty(ImageBase64) || ImageBase64.Length > 4)
            return false;
        return true;
    }

    public string ToSingleString() {
        return Merge.ListToString(new string[] {
            ImageBase64 ?? "",
            Password ?? ""
        });
    }
}