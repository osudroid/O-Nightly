namespace Rimu.ExtraInterface;

public interface ISecurity {
    public bool Api2HashValidate(string checkHash, string data, string token);

    public bool DecodeString(string content, string hashStr);

    public string EncryptString(string content);

    public string SignRequest(params string[] arr);

    public bool CheckRequest(string sign, params string[] arr);

    public bool CheckApiKey(string key);

    public bool CheckIfApiKeyIsRoot(string key);

    public bool CheckSudo(string key, string permissionName);
}