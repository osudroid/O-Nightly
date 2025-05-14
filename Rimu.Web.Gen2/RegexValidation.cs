using System.Text.RegularExpressions;

namespace Rimu.Web.Gen2;

public static class RegexValidation {
    private static readonly Regex _regexValidateEmail = new(
        "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])", 
        RegexOptions.Compiled
    );

    public static bool ValidateEmail(string email) => _regexValidateEmail.IsMatch(email);

    private static readonly Regex _regexValidateBase64 = new(
        "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$",
        RegexOptions.Compiled
    );
    
    public static bool ValidateBase64(string base64) => _regexValidateBase64.IsMatch(base64);

    private static readonly Regex _regexValidatHex = new("^([a-fA-F0-9]{2}\\s+)+", RegexOptions.Compiled);
    
    public static bool ValidateHex(string hex) => _regexValidatHex.IsMatch(hex);
}