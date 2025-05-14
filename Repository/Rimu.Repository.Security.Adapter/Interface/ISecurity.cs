namespace Rimu.Repository.Security.Adapter.Interface;

public interface ISecurity {
    public bool Api2HashValidate(string checkHash, string data, string token);

    public bool DecodeString(string content, string hashStr);

    public string EncryptString(string content);

    public string SignRequest(params Span<string> arr);

    public bool CheckRequest(string sign, params Span<string> arr);

    public bool CheckApiKey(string key);

    public bool CheckIfApiKeyIsRoot(string key);

    public bool CheckSudo(string key, string permissionName);
}